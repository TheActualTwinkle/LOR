#!/usr/bin/env python3
import os
import subprocess

def main():
    script_dir = os.path.dirname(__file__)
    docker_dir = os.path.join(script_dir, "docker")
    os.chdir(docker_dir)
    
    cmd = [
        "docker-compose",
        "--env-file", ".env",
        "-p", "lor",
        "-f", "docker-compose.dev.yml",
        "down"
    ]
    
    subprocess.run(cmd, check=True)

if __name__ == "__main__":
    main()