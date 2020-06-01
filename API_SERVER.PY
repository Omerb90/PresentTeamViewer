from flask import Flask
from flask import jsonify, request
from utils import *
app = Flask(__name__)
current_users = []
empty_state_response = {
    'response_code' : 0,
    'new_connections' : [],
    'new_disconnections' : [],
    'current_progroess' : {},
    'requesting_user' : '',
    'target_answer' : '',
}
server_data= {}
progress_connections = {}
# Key = Requesting User, Value = Trageted user

@app.route('/index',methods=['GET'])
def home():
    return "hello"

@app.route('/login',methods=['GET','POST'])
def login():
    content = request.json
    username = content.get('user')
    new_user = User(username)
    current_users.append(new_user)
    response = {"user_id" : new_user.user_id}
    response = jsonify(response)
    response.status_code = 200
    return response

@app.route('/get_state',methods=['POST'])
def get_state():
    content = request.json
    if not validate_user(content):
        return jsonify(empty_state_response)
    current_user = next((user for user in current_users if user.user == content.get('user')), None)
    if current_user:
        return jsonify(current_user.forge_response(current_users, progress_connections))
    else:
        return jsonify(empty_state_response)

@app.route('/answer_request', methods=['POST'])
def answer_request():
    content = request.json
    if not validate_user(content):
        return jsonify({'message':'error'})
    if content.get('target_answer'):
        current_username = next((u for u in progress_connections.values() if u == content.get('user')), None)
        current_user = next((user for user in current_users if user.user == current_username), None)
        current_user.answered_request = True
        current_user.peer.target_answer = content.get('target_answer')
        current_user.peer.ip = content.get('ip')
        current_user.peer.port = content.get('port')
        return jsonify({'message':'connection %s '%(content.get('target_answer')) })

    else:
        return jsonify({'message':'error'})

@app.route('/request_connection',methods=['POST'])
def request_connection():
    content = request.json
    if not validate_user(content):
        return jsonify({'message':'error'})
    current_user = next((user for user in current_users if user.user == content.get('user')), None)
    target_user = next((user for user in current_users if user.user == content.get('target_user')), None)
    if not current_user or not target_user or current_user==target_user:
        return jsonify({'message':'error'})
    else:
        target_user.set_target(current_user)
        current_user.set_requester(target_user)
        progress_connections[current_user.user] = target_user.user
        return jsonify({'message':'connection in progress'})
    

def validate_user(user_request):
    username = user_request['user']
    user = [user for user  in current_users if user == username]
    if not user:
        return False
    user = user[0]
    return user_request['user_id']==user.user_id

if  __name__ == "__main__":
    app.run(debug=True, host="192.168.1.13")
