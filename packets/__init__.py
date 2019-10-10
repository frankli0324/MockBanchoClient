from .packet import Packet
import packets.utils as utils


def unserialize(content: bytes) -> list:
    pointer = 0
    res = []
    while pointer < len(content):
        try:
            packet_len = utils.merge(content[pointer+3:pointer+7])
            res.append(Packet(content[pointer: pointer+packet_len]))
            pointer += packet_len+7
        except IndexError:
            raise EOFError('Unexpected End of Packet')
    return res


def serialize(packets: list) -> bytes:
    return b''
