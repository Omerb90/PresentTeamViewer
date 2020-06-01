import random
import string



class User():
    def __init__(self, user):
        self.user = user
        self.user_id = User.get_random_alphaNumeric_string()
        self.is_requesting = False
        self.is_target = False
        self.in_progress= False
        self.peer = None
        self.state = "waiting for request"
        self.currnet_progress = []
        self.connected_users = [user]
        self.ip = False
        self.port = False
        self.answered_request = False
        self.target_answer = False

    def set_requester(self,peer):
        self.is_requesting = True
        self.peer = peer
        self.in_progress = True

    def set_target(self,peer):
        self.is_target = True
        self.peer = peer
        self.in_progress = True


    @staticmethod
    def get_random_alphaNumeric_string(stringLength=16):
        lettersAndDigits = string.ascii_letters + string.digits
        return ''.join((random.choice(lettersAndDigits) for i in range(stringLength)))

    def __eq__(self,other):
        if isinstance(other,str):
            return self.user == other
        elif isinstance(other,unicode):
            return self.user == str(other)
        else:
            return False

    def update_user(self, **kwargs):
        if kwargs.get('current_connected'):
            current_connected = kwargs.get('current_connected')
            for user in current_connected:
                if user not in self.connected_users:
                    self.connected_users.append(user)
            for user in self.connected_users:
                if user not in current_connected:
                    self.connected_users.remove(user)

    def forge_response(self, current_connected, currnet_progress):
        #Checking for new conncetions
        response = dict()
        new_connections = [u.user for u in current_connected if u not in self.connected_users]
        new_disconnections = [u.user for u in self.connected_users  if u not in current_connected]
        self.currnet_progress = currnet_progress
        target_answer = ""
        requesting_user = ""
        ip = ""
        port=0
        if self.is_target and not self.answered_request:
            requesting_user = self.peer.user
        if self.is_requesting and self.target_answer:
            target_answer = self.target_answer
            ip = self.ip
            port = self.port
        
            #Incase the request was declined, the answer
            # is 'Declined' else, the answer shall 
            #iclude the ip and port of the target
        response = {
            'response_code' : 1,
            'new_connections' : new_connections,
            'new_disconnections' : new_disconnections,
            'current_progress' : self.currnet_progress,
            'requesting_user' : requesting_user,
            'target_answer' : target_answer,
            'ip':ip,
            'port':port

        }
        self.update_user(**{
            'current_connected' : current_connected
        })

        return response


        