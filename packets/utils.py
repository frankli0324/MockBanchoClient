def merge(content: bytes):
        s = 0
        for i in range(len(content)):
            s |= (content[i] << (8*i))
        return s
