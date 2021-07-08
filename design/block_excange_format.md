# Block eXchange Format (BXF)

The block exchange format (BXF) in SeedLang is a plaintext JSON format to
serialize, deserialize, import, and export SeedBlock programs.

A JSON object with the block exchange format represents a single SeedBlock code
module.

When a JSON object with the block exchange format is serialized and stored as a
disk file, the filename will conventionally end with `.bxf.json`.

The schema of the block exchange format is defined in
[bxf.schema.json](/schemas/bxf.schema.json), following the specification of
[JSON Schema](https://json-schema.org/). A number of [existing
validators](http://json-schema.org/implementations.html#validators) can be used
to check the correctness of BXF JSON files.
