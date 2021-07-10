# Block eXchange Format (BXF)

The block exchange format (BXF) in SeedLang is a plaintext JSON format to
serialize, deserialize, import, and export SeedBlock programs.

A JSON object with the block exchange format represents a single SeedBlock code
module.

## File extension

When a JSON object with the block exchange format is serialized and stored as a
disk file, the filename will conventionally end with `.bxf.json`.

## JSON Schema

The schema of the block exchange format is defined in
[bxf.schema.json](/schemas/bxf.schema.json), following the specification of
[JSON Schema](https://json-schema.org/).

## How to validate BXF JSON files?

A number of [existing
validators](http://json-schema.org/implementations.html#validators) can be used
to check the syntax correctness of BXF JSON files.

For example, here is a recommended way to validate BXF JSON files:

```shell
# Installs the JavaScript validator
npm install -g ajv-cli

# Validates an example JSON file
ajv --spec=draft2020 validate -s schemas/bxf.schema.json -d schemas/examples/single_statement.bxf.json
```

## Compressing and Packaging

When we store or transfer a number of SeedBlock modules as a whole package, the
BXF files should be compressed and archived in a ZIP file.

The ZIP file may have a directory structure. But the structure is just for
convenience purposes. Since all SeedBlock module names are globally visible, the
directory paths archived inside the ZIP file make NO impact on the semantic
meaning of module names.

For example, the modules of a robot program can be packaged in `ToyRobot.zip`
with the following internal structure:

```shell
ToyRobot
├── UserModules
│  ├── Main.bxf.json
│  ├── RobotActor.bxf.json
│  └── SceneObjects.bxf.json
└── ThirdPartyExtensions
   ├── ArduinoConnector.bxf.json
   └── SearchingAlgorithms.bxf.json
```
