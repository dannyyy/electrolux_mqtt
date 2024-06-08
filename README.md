![Docker Pulls](https://img.shields.io/docker/pulls/dannyyy/electrolux-mqtt)

# Electrolux to MQTT

<a href="https://www.buymeacoffee.com/danflash" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me A Coffee" height="60" width="217"></a>

## Description
The current release should support all smart Electrolux appliances, that are officially supported by the Electrolux app on [iOS](https://apps.apple.com/se/app/electrolux/id1595816832) and [Android](https://play.google.com/store/apps/details?id=com.electrolux.oneapp.android.electrolux).

Currently the *Electrolux to MQTT* integration is only tested for the following appliances:
| Device                                 | Tested | Features |
| -------------------------------------- | ------ | -------- |
| Electrolux Comfort 600 Air Conditioner | ✅     | - Supports all available status information<br>- Supports all commands |
| Electrolux Air Purifiers               | ？     | Not tested |
| Electrolux Robot Vacuums               | ？     | Not tested |
| Electrolux Laundry                     | ？     | Not tested |

*Please provide feedback to complete the compatibility table*

## Usage
### Available Appliances
Reports the found appliances every `<MQTT__STATUSUPDATEINTERVAL>` seconds to the topic `<TOPICPREFIX>appliances`.

e.g. `<TOPICPREFIX>=smarthome/electrolux/` -> `smarthome/electrolux/appliances`
```json
[
  {
    "ApplianeId": "1234567890",
    "ApplianceName": "My Air Conditioner",
    "ModelName":"Azul"
  }
]
```

### Appliance State
Reports the state of the appliance with some additional information to the topic `<TOPICPREFIX>appliances/<APPLIANCE_ID>/state`, which are not available on the app or appliance's control panel.

e.g. `<TOPICPREFIX>=smarthome/electrolux/` -> `smarthome/electrolux/appliances/1234567890/state`
```json
{
  "ambientTemperatureC": 27
  "targetTemperatureC": 23,
  "fanSpeedSetting": "high",
  "applianceState": "on",
  "compressorState": "off",
  "filterState":"good"
}
```

### Appliance Capabilities
Reports the available features with the allowed values to the topic `<TOPICPREFIX>appliances/<APPLIANCE_ID>/capabilities`. Using an unsupported feature or value will be ignored.

e.g. `<TOPICPREFIX>=smarthome/electrolux/` -> `smarthome/electrolux/appliances/1234567890/capabilities`
```json
{
  "targetTemperatureC": {
    "Min": 16,
    "Max": 32
  },
  "mode": {
    "Values": [
      "COOL",
      "FANONLY",
      "HEAT",
      "AUTO",
      "DRY",
      "OFF"
    ]
  },
  "fanSpeedSetting": {
    "Values": [
      "LOW",
      "HIGH",
      "MIDDLE",
      "AUTO"
    ]
  }
}
```

### Appliance Commands
Send one or more commands to the topic `<TOPICPREFIX>appliances/<APPLIANCE_ID>/command`. The order of the commands will be respected. After each command a state update will be published.

e.g. `<TOPICPREFIX>=smarthome/electrolux/` -> `smarthome/electrolux/appliances/1234567890/command`
```json
{
  "mode": "COOL",
  "targetTemperatureC": 23
}
```


## Release Notes
### 2.0.1
- Fixed an issue with duplicate appliance capabilities fetched from the Electrolux backend
### 2.0.0
- Open sourced on GitHub
- Removed source code protection
### 1.0.0
- Initial release with only MQTT 5 support (TCP/TLS)
- Reports registered appliances
- Reports capabilities and state
- Executes state changes

## Container Image
The image will follow a semantic versioning pattern. Where major updates will most probably introduce a breaking behavior. Minor updates will add new features or rearrange chunks of code behind the curtains. Hotfix releases contain fixes for bugs and minor changes.

New releaes will be tagged as `x.y.z`. A `x.y` tag will always reflect the latest hotfix version and is recommended to use.

### Docker
```bash
docker run \
  -d \
  -e MQTT__HOST="<HOST>" \
  -e MQTT__PORT="<PORT>" \                                      # (Default 8883)
  -e MQTT__USETLS="<USETLS>" \                                  # (Default true)
  -e MQTT__TLSSHA256FINGERPRINT="<SHA256>" \                    # If empty the certificate must be signed by a trusted CA
  -e MQTT__TRUSTALLCERTIFICATES="<TRUST-ALL>" \                 # If true every certificate will be considered as valid, even revoced and expired. (Default false)
  -e MQTT__USERNAME="<MQTT-USERNAME>" \
  -e MQTT__PASSWORD="<MQTT-PASSWORD>" \
  -e MQTT__TOPICPREFIX="<TOPIC-PREFIX-WITH-TRAILING-SLASH>" \   # Empty or with trailling slash e.g. "smarthome/electrolux/" (Default "")
  -e MQTT__STATUSUPDATEINTERVAL="<UPDATE-INTERAL-IN-S>" \       # Reports devices, capabilities and state (Default 30)
  -e ELECTROLUX__EMAIL="<EMAIL>" \
  -e ELECTROLUX__PASSWORD="<PASSWORD>" \
  dannyyy/electrolux-mqtt:2.0
```

### Kubernetes
```
---
apiVersion: v1
kind: Service
metadata:
  name: electrolux
  labels:
    app: electrolux

spec:
  ports:
    - protocol: TCP
      name: http
      port: 80
      targetPort: 8080
  selector:
    app: electrolux

---
kind: Deployment
apiVersion: apps/v1
metadata:
  name: electrolux
  labels:
    app: electrolux

spec:
  replicas: 1
  selector:
    matchLabels:
      app: electrolux
  template:
    metadata:
      labels:
        app: electrolux
    spec:
      containers:
        - name: electrolux
          image: dannyyy/electrolux-mqtt:2.0
          imagePullPolicy: Always
          ports:
            - containerPort: 8080
          env:
            - name: 'MQTT__HOST'
              value: ''
            - name: 'MQTT__PORT'
              value: ''
            - name: 'MQTT__USETLS'
              value: ''
            - name: 'MQTT__TLSSHA256FINGERPRINT'
              value: ''
            - name: 'MQTT__USERNAME'
              value: ''
            - name: 'MQTT__PASSWORD'
              value: ''
            - name: 'MQTT__TOPICPREFIX'
              value: ''
            - name: 'MQTT__STATUSUPDATEINTERVAL'
              value: ''
            - name: 'ELECTROLUX__EMAIL'
              value: ''
            - name: 'ELECTROLUX__PASSWORD'
              value: ''
```

## Home Assistant
An example integration on how to use it in Home Assistant.

For Home Assistant related questions and support, please use the community forum: https://community.home-assistant.io/t/electrolux-to-mqtt

```
climate:
    - name: <ENTITY NAME>
      device:
        identifiers:
          - '<APPLIANCEID>'
        manufacturer: 'Electrolux'
        model: '<YOUR MODEL>'
        name: '<YOUR NAME>'
        suggested_area: '<YOUR AREA>'

      availability_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/state'
      availability_template: '{{ value_json.connectionState }}'
      payload_available: 'connected'
      payload_not_available: 'disconnected'

      json_attributes_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/state'

      modes:
        - "auto"
        - "off"
        - "cool"
        - "heat"
        - "dry"
        - "fan_only"
      mode_command_topic: "smarthome/electrolux/appliances/<APPLIANCEID>/command"
      mode_command_template: '{ "mode":  "{{ "fanonly" if value == "fan_only" else value | upper }}" }'
      mode_state_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/state'
      mode_state_template: '{{ "fan_only" if value_json.mode == "fanonly" else value_json.mode | lower }}'

      precision: 1.0
      temperature_unit: 'C'
      initial: 22
      min_temp: 22
      max_temp: 25
      current_temperature_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/state'
      current_temperature_template: '{{ value_json.ambientTemperatureC }}'
      temperature_command_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/command'
      temperature_command_template: '{ "targetTemperatureC":  {{ value }} }'

      swing_modes:
        - "on"
        - "off"
      swing_mode_command_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/command'
      swing_mode_command_template: '{ "verticalSwing":  "{{ value | upper }}" }'
      swing_mode_state_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/state'
      swing_mode_state_template: '{{ value_json.verticalSwing }}'
      fan_modes:
        - "auto"
        - "high"
        - "medium"
        - "low"
      fan_mode_command_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/command'
      fan_mode_command_template: '{ "fanSpeedSetting":  "{{ "middle" if value =="medium" else value | upper }}" }'
      fan_mode_state_topic: 'smarthome/electrolux/appliances/<APPLIANCEID>/state'
      fan_mode_state_template: '{{ value_json.fanSpeedSetting }}'
```