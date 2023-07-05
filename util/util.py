from __future__ import annotations

import os
import time
import json
import inspect
from rich.console import Console

# if __name__ != "__main__":
# exit()

"""
    Easy reference to specific directories
"""
REPO_DIR = os.path.dirname(os.path.dirname(os.path.realpath(__file__)))
SERVER_DIR = os.path.join(REPO_DIR, "SocialCoder.Web", "Server")
SSL_DIR = os.path.join(REPO_DIR, "SSL")
SSL_TOOL = os.path.join(REPO_DIR, "SSL-Util")

CONNECTIONS_FILE = os.path.join(REPO_DIR, ".connections")
ENV_FILE = os.path.join(REPO_DIR, ".env")


class Reflection:
    def get_properties(self) -> [property]:
        return [attr for attr in dir(self) if isinstance(getattr(type(self), attr, None), property)]

    def to_env(self):
        return [f"{x.upper()}={getattr(self, x)}" for x in self.get_properties()]


class SSLSettings(Reflection):
    @property
    def cert_ca_auth_name(self):
        return self.__ca_auth_name

    @property
    def cert_ca_auth_pass(self):
        return self.__ca_auth_pass

    @property
    def cert_community_name(self):
        return self.__community_name

    @property
    def cert_days_valid(self):
        return self.__days_valid

    @property
    def cert_public_name(self):
        return self.__public_name

    def __init__(self, **kwargs):
        self.__ca_auth_name = kwargs.pop("CERT_CA_AUTH_NAME", "SocialCoderCA")
        self.__ca_auth_pass = kwargs.pop("CERT_CA_AUTH_PASS", "Testing1234")
        self.__community_name = kwargs.pop("CERT_COMMUNITY_NAME", "ProgrammingSimplifiedCommunity")
        self.__days_valid = kwargs.pop("CERT_DAYS_VALID", 180)
        self.__public_name = kwargs.pop("CERT_PUBLIC_NAME", "SocialCoder")


class DbSettings(Reflection):
    @property
    def db_path(self):
        return self.__db_path

    @property
    def db_password(self):
        return self.__db_password

    @property
    def db_name(self):
        return self.__db_name

    @property
    def db_host(self):
        return self.__db_host

    @property
    def db_user(self):
        return self.__db_user

    @property
    def db_docker_image(self):
        return self.__db_docker_image

    def __init__(self, **kwargs):
        self.__db_path = kwargs.pop("DB_PATH", os.path.join(REPO_DIR, "social-coder-api-db"))
        self.__db_password = kwargs.pop("DB_PASSWORD", "my-awesome-password")
        self.__db_name = kwargs.pop("social-coder")
        self.__db_host = kwargs.pop("localhost")
        self.__db_user = kwargs.pop("social-coder")
        self.__db_docker_image = kwargs.pop("postgres:latest")


console = Console()


def check_container_health(name: str, attempts: int = 10, interval: int = 5) -> bool:
    while attempts > 0:
        console.print(f"Waiting for [cyan]{name}[/] to be healthy")
        status = os.popen(f"docker ps --format \"table {{.Status}}\" --filter name=\"{name}\"").read().strip().lower()

        if "healthy" in status:
            return True

        attempts = attempts - 1
        time.sleep(interval)
    return False


def start_db(settings: DbSettings) -> None:
    console.print(f"Pulling [green]{settings.db_docker_image}[/] for social-coder-api-db")
    os.system(f"docker pull {settings.db_docker_image}")

    console.print(f"Starting [cyan]social-coder-api-db[/]...")
    os.system(f"docker compose --file \"{os.path.join(REPO_DIR, 'docker-compose.yml')}\" "
              f"--env-file \"{ENV_FILE}\" up -d \"social-coder-api-db\"")

    if not check_container_health("social-coder-api-db"):
        console.print("[red]Something went wrong trying to start database[/]. Unable to perform migrations")
        raise AssertionError()

    console.print("Running migrations for [cyan]social-coder-api-db[/]")
    os.system(f"dotnet ef database update --project \"{SERVER_DIR}\"")


def reset_db(settings: DbSettings) -> None:
    console.print("Tearing down [cyan]social-coder-api-db[/]")
    os.system(f"docker compose --env-file \"{ENV_FILE}\" --file \"{os.path.join(REPO_DIR, 'docker-compose.yml')} "
              f"rm -svg social-coder-api-db")

    if os.path.exists(settings.db_path):
        os.rmdir(settings.db_path)

    start_db(settings)


def build_ssl_container():
    existing_image = os.popen("docker images -q sslcontainer").read().strip().lower()

    if existing_image:
        console.print("[cyan]sslcontainer[/] already exists. Skipping build...")
        return

    os.system(f"docker build -t sslcontainer -f \"{os.path.join(SERVER_DIR, 'Dockerfile-ssl')}\" \"{SSL_TOOL}\"")


def create_ssl(settings: SSLSettings) -> None:
    build_ssl_container()
    env_vars = ' '.join([f"-e {x}" for x in settings.to_env()])
    os.system(f"docker run -v \"{SSL_DIR}:/certs\" {env_vars} sslcontainer")


def editor_for(title: str, settings: Reflection) -> Reflection:
    console.print()