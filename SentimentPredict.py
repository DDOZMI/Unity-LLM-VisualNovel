from flask import Flask, request, jsonify
from transformers import AutoTokenizer, AutoModelForSequenceClassification
import torch
import torch.nn.functional as F

app = Flask(__name__)

class SentimentAnalyzer:
    def __init__(self):
        self.MODEL_NAME = "ilmin/KcBERT_sentiment_v2.02"
        self.tokenizer = AutoTokenizer.from_pretrained(self.MODEL_NAME)
        self.model = AutoModelForSequenceClassification.from_pretrained(self.MODEL_NAME)
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        self.model.to(self.device)
        self.model.eval()
        
        # 감정 3가지로 분류, 4가지였는데 일단 이렇게만
        self.sentiment_map = {
            0: "negative",
            1: "neutral",
            2: "positive"
        }

    def predict_sentiment(self, text):
        # 입력 검증
        if not text or not isinstance(text, str):
            raise ValueError("Invalid input text")

        # 텍스트 전처리, 토큰화
        inputs = self.tokenizer(
            text,
            return_tensors="pt",
            truncation=True,
            max_length=256,
            padding=True
        ).to(self.device)

        # 감정 분류
        with torch.no_grad():
            outputs = self.model(**inputs)
            probabilities = F.softmax(outputs.logits, dim=-1)
            
        # 분류값 최대치 보고 감정 추출
        predicted_class = torch.argmax(probabilities).item()
        confidence = probabilities[0][predicted_class].item()
        
        # GPU 메모리 정리
        if torch.cuda.is_available():
            torch.cuda.empty_cache()
        
        return {
            'sentiment': self.sentiment_map[predicted_class],
            'confidence': float(confidence),
            'probabilities': {
                'negative': float(probabilities[0][0]),
                'neutral': float(probabilities[0][1]),
                'positive': float(probabilities[0][2])
            }
        }


analyzer = SentimentAnalyzer()

@app.route('/analyze_sentiment', methods=['POST'])
def analyze_sentiment():
    try:
        data = request.json
        text = data.get('text', '')
        
        if not text:
            return jsonify({'error': 'Text is required'}), 400
            
        # 감정 분석 수행
        result = analyzer.predict_sentiment(text)
        
        return jsonify({
            'text': text,
            **result
        })
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5001)