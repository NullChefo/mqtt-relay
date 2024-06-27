#!/bin/bash

dotnet publish -c Release -r linux-x64 /p:PublishTrimmed=true /p:PublishSelfContained=true /p:ContainerFamily=jammy-chiseled-extra /p:ContainerRepository=mqtt-relay-chiseled /t:PublishContainer



dotnet publish -c Release -r linux-arm64 /p:PublishTrimmed=true /p:PublishSelfContained=true /p:ContainerFamily=jammy-chiseled /p:ContainerRepository=mqtt-relay-chiseled /t:PublishContainer


docker run  mqtt-relay-chiseled:latest