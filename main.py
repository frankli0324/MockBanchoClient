from client import BanchoClient
from packets import unserialize

ses = BanchoClient()
ses.login('username', 'password')
ses.poll()