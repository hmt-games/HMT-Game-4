import requests
from json import loads
from json import dumps
from json.decoder import JSONDecodeError
from pprint import pprint
import random
import time
import argparse
import sys
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

        resp = requests.post('http://' + root_url + '/register_agent', json=data)

        if resp.status_code != 200:
            print('Register Agent Response Status:', resp.status_code)
            print(resp.text)
            self.connection = None
        else:
            resp_json = resp.json()
            print('Register Agent Response')
            pprint(resp_json)

            self.service_target = resp_json['service_target']
            self.session_id = resp_json['session_id']
            self.agent_id = resp_json['agent_id']
            self.puppet_id = resp_json['puppet_id']
            self.priority = resp_json['priority']
            self.action_set = resp_json['action_set']
            self.api_key = resp_json.get('api_key', '')

            self.connection = connect(self.service_target,
                                      open_timeout=None, close_timeout=None)

    def execute_action(self, action, params=None):
        # Command to send to Game env
        if params is None:
            action_command = {"command": "execute_action",
                              "action": action}
        else:
            action_command = {"command": "execute_action",
                              "action": action,
                              "params": params}

        return self.send(action_command)

    def execute_plan(self, plan):
        plan_command = {"command": "execute_plan",
                        "plan": plan}

        return self.send(plan_command)

    def get_state(self):
        command = {"command": "get_state"}
        return self.send(command)

    def send(self, mess):
        if self.connection is None:
            return None
        else:
            if self.api_key != '':
                mess['api_key'] = self.api_key
            mess = dumps(mess)
            self.connection.send(mess)
            resp = self.connection.recv()
            print(resp)
            return loads(resp)


def test_random_walk(socket, waitTime=1, iterations=None):
    directions = ["up", "down", "left", "right"]

    if iterations is None:
        iterations = float('inf')
    it = 0
    while it < iterations:
        print('Interation: ', it)
        state = socket.get_state()
        pprint(state)
        direct = random.choice(directions)
        resp = socket.execute_action("move", {"direction": direct})
        pprint(resp)
        time.sleep(waitTime)


if __name__ == "__main__":

    parser = argparse.ArgumentParser(
        prog='Test Agent',
        description='A simple agent for testing the HMT Agent Interface')

    parser.add_argument(
        'root_url', help='The root url for the agent service. likely localhost:4649/hmt')
    parser.add_argument('-p', '--puppet_id', help='The puppet_id to register actions on')
    parser.add_argument('-a', '--agent_id', default='TEST',
                        help='The agent id to register, can be any string')
    parser.add_argument('-l', '--list_puppets', action='store_true',
                        help='Overrides other params and calls the list_puppets target')
    parser.add_argument('-r', '--priority', type=int, default=128,
                        help='The priorty the agent should perform actions with')
    parser.add_argument('-s', '--style', choices=['random-walk', 'random'],
                        default='random-walk', help='What kinds of actions do you want the agnet to test')
    parser.add_argument('-w', '--waitTime', type=float, default=1.0,
                        help='How long to wait between sending actions')
    parser.add_argument('-i', '--iterations', type=int, default=None,
                        help='How many iterations of actions to send. Default is to loop infinitely')

    args = parser.parse_args()

    if args.list_puppets:
        print('Calling /list_puppets API')
        try:
            resp = requests.get('http://' + args.root_url + '/list_puppets')
        except:
            print('Error on request, server is probably not running.')
            sys.exit()

        if resp.status_code == 200:
            try:
                resp_json = resp.json()
                print('Retrieved Puppets:')
                for puppet in resp_json['puppets']:

                    print(' - ' + puppet['puppet_id'])
                    print('     - actions:' + str(puppet['action_set']))
            except JSONDecodeError:
                print('Response Format is Bad')
                print(resp)
        else:
            print('Response Status:', resp.status_code)
    else:
        socket = UnityWebSocket(args.root_url, args.agent_id,
                                args.puppet_id, args.priority)

        if socket.connection:

            if args.style == 'random-walk':
                print('Launching random walk agent')
                test_random_walk(socket, args.waitTime, args.iterations)
            elif args.stype == 'random':
                pass

        else:
            print('Socket failed to intialize')