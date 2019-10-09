from hashlib import md5
import requests


class BanchoClient(requests.Session):

    DOMAIN = 'https://c4.ppy.sh'

    CLIENT_VERSION = 'b20191006.1'
    TIMEZONE = '8'
    P_UNKNOWN = '0'
    EXECUTABLE_HASH = '6e9fd4b6476e70d0760d88d4cca616ec'
    REMARKS = ''

    @staticmethod
    def _md5(s: str):
        return md5(s.encode()).hexdigest()

    def request(self, *args, **kwargs):
        resp = super().request(*args, **kwargs)
        self.cookies.clear()
        return resp

    def __init__(self, username: str, password: str, check_latest=False):
        super().__init__()
        self.headers.clear()
        self.headers.update({
            'User-Agent': 'osu!',
            'Accept-Encoding': 'gzip, deflate',
            'Connection': 'Keep-Alive'
        })
        if check_latest:
            for i in self.get('https://osu.ppy.sh/web/check-updates.php', params={
                'action': 'check',
                'stream': 'stable40',
                # 'time': ?
            }).json():
                if i['filename'] == 'osu!.exe':
                    self.EXECUTABLE_HASH = i['file_hash']

        metadata = ':'.join([
            self.EXECUTABLE_HASH,
            self.REMARKS,
            self._md5(self._md5('')),
            self._md5(self._md5('unknown')),
            self._md5(self._md5('unknown'))
        ])+':'

        self.country_code = self.get('https://osu.ppy.sh/web/bancho_connect.php', params={
            'v': self.CLIENT_VERSION,
            'u': username,
            'h': self._md5(password),
            'fx': 'dotnet4|dotnet4',
            'ch': metadata
        }).text
        login_data = '\n'.join([
            username, self._md5(password),
            '|'.join([
                self.CLIENT_VERSION,
                self.TIMEZONE,
                self.P_UNKNOWN,
                metadata,
                '0'
            ])
        ])+'\n'
        print(login_data)
        cho_token = ''
        while cho_token == '':
            try:
                res = self.post(self.DOMAIN, headers={
                    'osu-version': self.CLIENT_VERSION,
                }, data=login_data)
                assert(res.status_code == 200)
                cho_token = res.headers['cho-token']
                print(res.content)
            except AssertionError:
                pass
        self.headers.update({'osu-token': cho_token})
