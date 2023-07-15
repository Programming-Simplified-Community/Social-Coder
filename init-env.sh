#!/bin/bash

# Parse named arguments
OPTIONS=d:p:n:h:
LONGOPTS=databasePath:,dbPassword:,dbName:,dbHost:
PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name ="$0" -- "$@")

if [[ $? -ne 0 ]]; then
  exit 1
fi 

eval set -- "$PARSED"

# Set Default Values

databasePath=""
dbPassword=""
dbName=""
dbHost=""

while true; do
  case "$1" in
        -d|--databasePath)
          databasePath="$2"
          shift 2
          ;;
        -p|--dbPassword)
          dbPassword="$2"
          shift 2
          ;;
        -n|--name)
          dbName="$2"
          shift 2
          ;;
        -h|--dbHost)
          dbHost="$2"
          shift 2
          ;;
        --)
          shift
          break
          ;;
        *)
          echo "Invalid argument: $1"
          exit 1
          ;;
  esac
done

declare -A vars

# we are going to allow a developer to override default values
# default values are stored in the '.defaults' file

if [ -n "$databasePath" ]; then
  vars["DB_PATH"]=$databasePath
fi 

if [ -n "$dbPassword" ]; then
  vars["DB_PASSWORD"]=$dbPassword
fi 

if [ -n "$dbName" ]; then
  vars["DB_NAME"]=$dbName
fi

if [ -n "$dbHost" ]; then
  vars["DB_HOST"]=$dbHost
fi

if [ -z "$databasePath" ]; then
  databasePath="$(dirname "$0")/Database"
fi

vars["DB_PATH"]=$databasePath

# Similar to powershell we're sourcing our scripts from our submodule
funcPath="$(dirname "$0")/Scripts/funcs.sh"
source "$funcPath"

# pass our dictionary of values
createEnv "${vars[@]}" --allowDefaults true

# we need to load our env file so we know which values got used...
loadEnvFile

# create our connection string basedo n the values in our .env
connectionEnvPath="$(dirname "$0")/.connections"
connectionString="Server=${DB_HOST};port=5432;User Id=${DB_USER};Password=$(DB_PASSWORD);Database=${DB_NAME}"
createEnv "DefaultConnection=${connectionString}" -saveTo "$connectionEnvPath"