from .utils import merge

class Packet:

    def __init__(self, content: bytes):
        self.type = merge(content[0:2])
        self.len = merge(content[3:7])
        self.body = content[7:]

