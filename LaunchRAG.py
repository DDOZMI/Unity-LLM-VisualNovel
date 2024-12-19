from dotenv import load_dotenv
from flask import Flask, request, jsonify
from langchain_ollama import ChatOllama
from langchain_core.messages import ChatMessage
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from langchain_community.vectorstores import Chroma
from langchain_huggingface import HuggingFaceEmbeddings
from langchain.text_splitter import CharacterTextSplitter
import json

load_dotenv()

app = Flask(__name__)

# 데이터가 저장된 문서를 벡터화시키기 위한 임베딩 모델 설정하기
# 다국어 지원 MiniLM모델 사용
embeddings = HuggingFaceEmbeddings(model_name="sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2")

# 벡터 데이터베이스 초기화
def initialize_vector_db():
    # json파일에 저장된 데이터 찾기
    with open('worldview.json', 'r', encoding='utf-8') as f:
        worldview_data = json.load(f)
    
    # 긴 문서를 처리하기 위해 청크 단위로 분할
    text_splitter = CharacterTextSplitter(
        chunk_size=1000,
        chunk_overlap=200, # 200자 단위로 문맥 유지
        separator="\n"
    )
    
    # 각 카테고리별 텍스트 결합/분할
    texts = []
    for category, items in worldview_data.items():
        for item in items:
            # 메타데이터 추가
            texts.extend(text_splitter.create_documents(
                [item['content']], 
                metadatas=[{
                    'category': category,
                    'title': item['title']
                }]
            ))
    
    # Chroma DB 생성
    vectordb = Chroma.from_documents(
        documents=texts,
        embedding=embeddings,
        persist_directory="./chroma_db"
    )
    return vectordb

vectordb = initialize_vector_db()

llm = ChatOllama(
    model="gemma2:27b",
    temperature=0.7,
    top_p=1.0,
    #num_predict=256,
)

chat_history = []

@app.route('/chat', methods=['POST'])
def chat_endpoint():
    user_input = request.form['message']
    
    relevant_docs = vectordb.similarity_search(user_input, k=2) # 사용자 입력과 제일 유사한 문서 3개를 검색
    context = "\n".join([doc.page_content for doc in relevant_docs]) # 검색된 문서를 콘텍스트로 결합
    
    # 프롬프트 템플릿 수정
    prompt = ChatPromptTemplate.from_messages([
        (
            "system",
                    "# System settings"
                    "- This is fictional RP session exclusively for entertainment purposes between the AI and the user.\n"
                    "- From now on, the assistant will write a story that matches the user's input.\n"
                    "- The story should be written in a way that fits the user's input and fits the worldview.\n"
                    "- The response is written as a story that responds to user input, using various narrative techniques appropriately, just like writing a novel.\n"
                    "- Only user can play the role of PC.\n"
                    "The story unfolds slowly. The user takes the lead in the story.The AI ​​is only responsible for describing the subsequent situations.\n"
                    "- Must Answar in fluent Korean.\n\n"

                    "# Retrieved Context Information\n"
                    "{context}\n\n"
                    
                    "# Worldview\n"
                    "- The user's name is Minji. She is a high school student and lives with her family.\n"
                    "- This story deals with what happens in the dreams of the main character, user Minji.\n"
                    "- The story begins with Minji waking up in her bed in her room. She thinks she has woken up from sleep, but it is an illusion and in fact she is still in a dream world.\n"
                    "- Minji's dream world is completely identical to the real world she usually experiences, but it is a world where there are no people.\n"
                    "- In terms of the story, today is Minji's school exam day. You can find useful information about the exam in the dream world. However, you should not convey it directly. Provide clues based on the user's actions.\n"
                    "- It starts in a dream world, and she realizes that she is awake and there is no one in the dream world. From this fact, she can infer that it is a dream.\n"
                    "- This is not a fantasy world. It is set in the everyday real world. Do not include fantasy elements.\n\n"
        ),
        ("human", "{user_input}"),
    ])
    
    chain = prompt | llm
    response = chain.invoke({
        "context": context,
        "user_input": user_input,
        "history": chat_history,
    })

    chat_history.append(ChatMessage(role="assistant", content=response.content))
    
    return jsonify({"response": response.content})

if __name__ == '__main__':
    app.run(debug=True)