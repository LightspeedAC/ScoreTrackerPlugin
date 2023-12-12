# ScoreTrackerPlugin
An [AssettoServer](https://github.com/compujuckel/AssettoServer) plugin that will track overtake scores, drift scores, and lap times. Each player and their top score/lap time will be stored in the root folder of your server under `score-tracker`. The plugin will then display all those entries in json content on your servers web page at `http://{ip}:{port}/scores` or `http://{ip}:{port}/laptimes`. Which can be access by Discord bots, etc.

#### Note: Lap times aren't fully implemented, it seemed to work on Nordschleife so I stopped there. Should any problems arrise I can do my best to fix them.

### Server Configuration
Enable CSP client messages in your `extra_cfg.yml`
```YAML
EnableClientMessages: true
```

Enable the plugin in your `extra_cfg.yml`
```YAML
EnablePlugins:
- ScoreTrackerPlugin
```

Add the plugin configuration to the bottom of your `extra_cfg.yml`
```YAML
---
!ScoreTrackerConfiguration
# Whether to listen for overtake score, drift score, or timed laps. 0 = overtake score, 1 = drift score, 2 = timed laps
ServerType: 1
# Depending on 'ServerType', the server sends a message in chat for each new personal best overtake score, drift score, or lap time.
BroadcastMessages: true
```

### Lua Script Configuration (Overtake/Drift)

Add this `OnlineEvent` to your Lua script
##### Keep the structure the same otherwise the plugin won't capture any scores. For drift scores just rename the key to `driftScoreEnd`.
```lua
local msg = ac.OnlineEvent({
    ac.StructItem.key("overtakeScoreEnd"),
    Score = ac.StructItem.int64(),
    Multiplier = ac.StructItem.int32(),
    Car = ac.StructItem.string(64),
})
```
Send a message using the `OnlineEvent`
```lua
msg{ Score = personalBest, Multiplier = comboMeter, Car = ac.getCarName(0) }
```

### Example Usage Screenshots
![alt text](https://i.imgur.com/yVbRKyN.png "Overtake scores being used in a Discord.py bot")
![alt text](https://i.imgur.com/wrKlVJd.png "Lap times being used in a Discord.py bot")
