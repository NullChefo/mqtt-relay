#!/bin/bash

dotnet publish -c Release /p:PublishTrimmed=true -r linux-x64 /p:PublishSelfContained=true /p:ContainerFamily=jammy-chiseled-extra /p:ContainerRepository=mqtt-relay-chiseled /t:PublishContainer