import requests
from json import loads
from websockets.sync.client import connect


class UnityWebSocket:

    def __init__(self, root_url, agent_id, puppet_id, priority=None, session_id=None):
        # self.connection = connect(url, open_timeout=None, close_timeout=None)
        data = {"agent_id": agent_id,
                "puppet_id": puppet_id}

        if priority is not None:
            data["priority"] = priority
        if session_id is not None:
            data["session_id"] = session_id

        resp = requests.post(root_url + '/register_agent', json=data)
        resp_json = resp.json()

        self.service_target = resp_json['service_target']
        self.session_id = resp_json['session_id']
        self.agent_id = resp_json['agent_id']
        self.puppet_id = resp_json['puppet_id']
        self.priority = resp_json['priority']
        self.action_set = resp_json['action_set']
        self.api_key = resp_json.get('api_key', '')

        self.connection = connect(self.service_target,
                                  open_timeout=None, close_timeout=None)

    def execute_action(self, action):
        # Command to send to Game env
        action_command = {"command": "execute_action",
                          "action": action}
        return self.send(str(action_command))

    def execute_plan(self, plan):
        plan_command = {"command": "execute_plan",
                        "plan": plan}

        return self.send(str(plan_command))

    def get_state(self):
        return self.send('{"command":"get_state"}')

    def send(self, message):
        if self.connection is None:
            return None
        else:
            self.connection.send(message)
            return loads(self.connection.recv())


"""

def test_get_state(players, save_to_file=False):
    for p in players:
        skt = UnityWebSocket(url=f"ws://localhost:4649/hmt/{p}")
        state = skt.get_state(version='fog')
        print(f'State for character ({p}): {state}')
        if save_to_file:
            with open(f"test-state-{p}.json", "w") as file:
                file.write(dumps(state, indent=2))


def test_pinging():
    action_list = [
        ("giant", ["up", "up", "pinga", "submit"]),
        ("human", ["down", "down", "pingb", "submit"]),
        ("dwarf", ["right", "pingc"]),
        ("dwarf", ["left", "left", "pingd", "submit"])
    ]
    for char, actions in action_list:
        skt = UnityWebSocket(url=f"ws://localhost:4649/hmt/{char}")
        for action in actions:
            skt.execute_action(action)
            

def test_action_planning(players):
    for p in players:
        skt = UnityWebSocket(url=f"ws://localhost:4649/hmt/{p}")
        actions = ['left', 'down', 'right', 'up']
        skt.execute_action(choice(actions))
        # Auto submit
        skt.execute_action('submit')

players = ['dwarf', 'giant', 'human']
# Test Get State
test_get_state(players)
# Test Get State
test_pinging()
# Test Execute Action
test_execute_action(players)
"""