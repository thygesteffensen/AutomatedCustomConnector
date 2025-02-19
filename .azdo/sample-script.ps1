$connectorId = "<connector-guid>" # The guid - Use `pac connector list` to get the guid
$environment = "https://<org>.crm4.dynamics.com" # D365CE - MDM - Udvikling - Use `pac env list` to get the URL 

Set-Location -Path ..

New-Item -ItemType Directory -Path out

dotnet build --configuration Release

jq 'del(.definitions.securitySchemes, .security)' src/WebApi/WebApi_cc.json > out/WebApi_cc.json


cp ../temp.json out/apiDefinition.json
#pac env select --environment $environment
#pac connector download --connector-id $connectorId --outputDirectory out


jq 'del(.paths, .info, .definitions)' out/apiDefinition.json > out/base.json

Remove-Item out/apiDefinition.json

jq -s '.[0] + .[1]' out/base.json out/WebApi_cc.json > out/apiDefinition.json

cat out/apiDefinition.json

#pac connector update --environment $environment --connector-id connectorId --api-definition-file out/apiDefinition.json --api-properties-file out/apiProperties.json --icon-file out/icon.png
#
#pac solution publish
