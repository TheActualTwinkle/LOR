#!/usr/bin/env python3
import os
import subprocess

def main():
    os.chdir(os.path.join(os.path.dirname(__file__), "docker"))
    
    cmd = [
        "docker-compose",
        "--env-file", ".env",
        "-p", "lor",
        "-f", "docker-compose.dev.yml",
        "up", "-d", "--build",
    ]
    
    subprocess.run(cmd, check=True)

if __name__ == "__main__":
    main()