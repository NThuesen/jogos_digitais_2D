#!/usr/bin/env bash
#
# Writes the whole sound set into Assets/_Game/Resources/Audio/.
#
# Nothing here is recorded or downloaded. Every file is produced by the studio's audio
# server from an explicit stack of voices, which is why the project ships with no audio
# licence to track and why the same command always writes byte-identical files.
#
# Run it again after changing a sound, then measure the result:
#   node ../agent_creater/tools/mcp/scripts/call.mjs audio sfx_measure \
#     '{"directory":"Assets/_Game/Resources/Audio"}'
#
set -euo pipefail

HERE="$(cd "$(dirname "$0")/.." && pwd)"
CALL="$HERE/../agent_creater/tools/mcp/scripts/call.mjs"
OUT="$HERE/Assets/_Game/Resources/Audio"

mkdir -p "$OUT"

say () { printf '  %-18s %s\n' "$1" "$2"; }

synth () { # $1 name  $2 seconds  $3 voices-json  [$4 extra-json]
  local extra="${4:-}"
  local json="{\"output\":\"$OUT/$1.wav\",\"durationSeconds\":$2,\"voices\":$3${extra:+,$extra}}"
  node "$CALL" audio sfx_synthesise "$json" > /dev/null
  say "$1" "ok"
}

preset () { # $1 name  $2 preset  $3 rootHz
  node "$CALL" audio sfx_from_preset \
    "{\"output\":\"$OUT/$1.wav\",\"preset\":\"$2\",\"rootHz\":$3}" > /dev/null
  say "$1" "$2"
}

echo "Efeitos"

# The door. Two bells a fifth apart, the second a touch late: a shop doorbell, not a chime.
synth door_bell 0.9 '[
  {"waveform":"sine","frequency":1046.5,"amplitude":0.5,
   "envelope":{"attack":0.002,"decay":0.35,"sustain":0.05,"release":0.4}},
  {"waveform":"sine","frequency":1568.0,"amplitude":0.32,"delay":0.07,
   "envelope":{"attack":0.002,"decay":0.3,"sustain":0.04,"release":0.45}},
  {"waveform":"triangle","frequency":2093.0,"amplitude":0.14,"delay":0.07,
   "envelope":{"attack":0.002,"decay":0.18,"sustain":0.0,"release":0.2}}
]' '"space":0.25'

# A hand going up. A short rising blip, nothing more.
synth order_pop 0.22 '[
  {"waveform":"sine","frequency":620,"frequencyEnd":1180,"amplitude":0.6,
   "envelope":{"attack":0.004,"decay":0.09,"sustain":0.0,"release":0.09}}
]'

# Paper on a rail.
synth ticket_slap 0.16 '[
  {"waveform":"noise","frequency":1000,"amplitude":0.45,"lowPassHz":2600,
   "envelope":{"attack":0.001,"decay":0.06,"sustain":0.0,"release":0.07}},
  {"waveform":"triangle","frequency":280,"frequencyEnd":180,"amplitude":0.25,
   "envelope":{"attack":0.001,"decay":0.05,"sustain":0.0,"release":0.06}}
]'

# A pan. Filtered noise with a slow swell, so it reads as cooking and not as static.
# The first version sat at 7200 Hz, seven times the brightness of the rest of the set:
# hiss, played several times a minute. The low pass came down to 1500 and the body up.
synth sizzle 0.75 '[
  {"waveform":"noise","frequency":1000,"amplitude":0.12,"lowPassHz":900,
   "envelope":{"attack":0.08,"decay":0.2,"sustain":0.45,"release":0.3}},
  {"waveform":"noise","frequency":1000,"amplitude":0.34,"lowPassHz":380,
   "envelope":{"attack":0.05,"decay":0.3,"sustain":0.45,"release":0.3}},
  {"waveform":"triangle","frequency":150,"amplitude":0.18,"lowPassHz":400,
   "envelope":{"attack":0.1,"decay":0.3,"sustain":0.35,"release":0.3}}
]'

# A plate set down: the one sound a player hears hundreds of times, so it is soft.
synth plate_chime 0.5 '[
  {"waveform":"sine","frequency":1318.5,"amplitude":0.42,
   "envelope":{"attack":0.002,"decay":0.22,"sustain":0.0,"release":0.26}},
  {"waveform":"sine","frequency":1975.5,"amplitude":0.2,"delay":0.015,
   "envelope":{"attack":0.002,"decay":0.16,"sustain":0.0,"release":0.2}}
]' '"space":0.2'

# The till.
synth register 0.55 '[
  {"waveform":"triangle","frequency":1174.66,"amplitude":0.4,"lowPassHz":2600,
   "envelope":{"attack":0.002,"decay":0.1,"sustain":0.0,"release":0.12}},
  {"waveform":"triangle","frequency":1760,"amplitude":0.26,"delay":0.05,"lowPassHz":2600,
   "envelope":{"attack":0.002,"decay":0.12,"sustain":0.0,"release":0.18}},
  {"waveform":"sine","frequency":392,"amplitude":0.22,
   "envelope":{"attack":0.002,"decay":0.14,"sustain":0.0,"release":0.16}},
  {"waveform":"noise","frequency":1000,"amplitude":0.1,"lowPassHz":1800,
   "envelope":{"attack":0.001,"decay":0.05,"sustain":0.0,"release":0.05}}
]' '"space":0.3'

synth coin 0.3 '[
  {"waveform":"square","frequency":1046.5,"amplitude":0.22,
   "envelope":{"attack":0.002,"decay":0.05,"sustain":0.0,"release":0.06}},
  {"waveform":"square","frequency":1568.0,"amplitude":0.2,"delay":0.055,
   "envelope":{"attack":0.002,"decay":0.09,"sustain":0.0,"release":0.12}}
]'

# Somebody leaving happy.
synth heart 0.45 '[
  {"waveform":"sine","frequency":880,"frequencyEnd":1318.5,"amplitude":0.34,
   "envelope":{"attack":0.02,"decay":0.18,"sustain":0.1,"release":0.2},
   "vibrato":{"depth":0.12,"rateHz":7}}
]'

# Somebody leaving unhappy. Falls, and is short enough not to be funny the tenth time.
synth angry_puff 0.3 '[
  {"waveform":"noise","frequency":1000,"amplitude":0.35,"lowPassHz":1400,
   "envelope":{"attack":0.004,"decay":0.12,"sustain":0.0,"release":0.12}},
  {"waveform":"saw","frequency":320,"frequencyEnd":150,"amplitude":0.22,"lowPassHz":1200,
   "envelope":{"attack":0.004,"decay":0.1,"sustain":0.0,"release":0.12}}
]'

# The clatter. Measured 2.2x quieter than the median on the first pass, which for the
# sound of a plate hitting the floor is the wrong way round.
synth drop 0.35 '[
  {"waveform":"noise","frequency":1000,"amplitude":0.4,"lowPassHz":2600,
   "envelope":{"attack":0.001,"decay":0.14,"sustain":0.12,"release":0.14}},
  {"waveform":"triangle","frequency":520,"frequencyEnd":220,"amplitude":0.45,
   "envelope":{"attack":0.001,"decay":0.14,"sustain":0.1,"release":0.14}},
  {"waveform":"sine","frequency":150,"amplitude":0.3,
   "envelope":{"attack":0.001,"decay":0.16,"sustain":0.0,"release":0.16}}
]'

# Levelling up. A rising major triad, the only fanfare in the game.
synth level_up 1.1 '[
  {"waveform":"triangle","frequency":523.25,"amplitude":0.34,
   "envelope":{"attack":0.004,"decay":0.16,"sustain":0.0,"release":0.18}},
  {"waveform":"triangle","frequency":659.25,"amplitude":0.34,"delay":0.1,
   "envelope":{"attack":0.004,"decay":0.16,"sustain":0.0,"release":0.18}},
  {"waveform":"triangle","frequency":783.99,"amplitude":0.34,"delay":0.2,
   "envelope":{"attack":0.004,"decay":0.16,"sustain":0.0,"release":0.2}},
  {"waveform":"sine","frequency":1046.5,"amplitude":0.4,"delay":0.3,
   "envelope":{"attack":0.004,"decay":0.3,"sustain":0.1,"release":0.4}}
]' '"space":0.35'

echo "Interface"
preset ui_tick blip 1174.66
# The sparkle preset measured 2.6x brighter than the rest of the set, and this is the
# sound every menu press makes. Replaced with a soft two-note rise.
synth ui_confirm 0.3 '[
  {"waveform":"sine","frequency":587.33,"amplitude":0.4,"lowPassHz":2200,
   "envelope":{"attack":0.004,"decay":0.09,"sustain":0.0,"release":0.1}},
  {"waveform":"sine","frequency":880.00,"amplitude":0.3,"delay":0.055,"lowPassHz":2200,
   "envelope":{"attack":0.004,"decay":0.12,"sustain":0.0,"release":0.13}}
]'
synth ui_back 0.2 '[
  {"waveform":"sine","frequency":660,"frequencyEnd":392,"amplitude":0.4,
   "envelope":{"attack":0.003,"decay":0.08,"sustain":0.0,"release":0.09}}
]'

echo "Vozes"
# One blip per archetype. Same shape, different pitch, sweep and vibrato — which is
# enough to tell nine people apart with your eyes on another part of the room.
nya () { # $1 name  $2 start  $3 end  $4 vibrato-depth  $5 rate  $6 seconds
  synth "nya_$1" "$6" "[
    {\"waveform\":\"triangle\",\"frequency\":$2,\"frequencyEnd\":$3,\"amplitude\":0.45,
     \"lowPassHz\":3200,
     \"envelope\":{\"attack\":0.012,\"decay\":0.09,\"sustain\":0.25,\"release\":0.12},
     \"vibrato\":{\"depth\":$4,\"rateHz\":$5}},
    {\"waveform\":\"sine\",\"frequency\":$3,\"amplitude\":0.16,\"delay\":0.04,
     \"envelope\":{\"attack\":0.01,\"decay\":0.1,\"sustain\":0.0,\"release\":0.12}}
  ]"
}

nya genki     780  1180 0.18 9.0 0.30
nya kuudere   520   470 0.03 4.0 0.34
nya tsundere  700   900 0.14 7.5 0.26
nya dojikko   640  1040 0.22 11.0 0.32
nya ojousama  880  1040 0.08 5.5 0.38
nya oneesan   560   700 0.10 5.0 0.40
nya idol      920  1320 0.20 8.5 0.42
nya gourmand  480   600 0.12 6.0 0.36
nya mukuchi   600   560 0.05 3.5 0.22

echo "Trilhas"
# Eight-second loops, sixteen voices each, placed by delay. Sparse on purpose: a café bed
# that competes with the till is a café bed nobody can work over.
synth music_menu 8.0 '[
  {"waveform":"sine","frequency":261.63,"amplitude":0.22,"delay":0.0,"duration":3.6,
   "envelope":{"attack":0.4,"decay":1.0,"sustain":0.4,"release":1.6},"lowPassHz":1400},
  {"waveform":"sine","frequency":329.63,"amplitude":0.18,"delay":0.5,"duration":3.2,
   "envelope":{"attack":0.4,"decay":1.0,"sustain":0.35,"release":1.4},"lowPassHz":1400},
  {"waveform":"sine","frequency":392.00,"amplitude":0.16,"delay":1.0,"duration":3.0,
   "envelope":{"attack":0.4,"decay":1.0,"sustain":0.3,"release":1.4},"lowPassHz":1400},
  {"waveform":"triangle","frequency":523.25,"amplitude":0.1,"delay":1.6,"duration":1.2,
   "envelope":{"attack":0.02,"decay":0.5,"sustain":0.0,"release":0.6}},
  {"waveform":"triangle","frequency":659.25,"amplitude":0.09,"delay":2.4,"duration":1.2,
   "envelope":{"attack":0.02,"decay":0.5,"sustain":0.0,"release":0.6}},
  {"waveform":"sine","frequency":220.00,"amplitude":0.2,"delay":4.0,"duration":3.6,
   "envelope":{"attack":0.4,"decay":1.0,"sustain":0.4,"release":1.6},"lowPassHz":1200},
  {"waveform":"sine","frequency":293.66,"amplitude":0.17,"delay":4.5,"duration":3.2,
   "envelope":{"attack":0.4,"decay":1.0,"sustain":0.35,"release":1.4},"lowPassHz":1300},
  {"waveform":"triangle","frequency":440.00,"amplitude":0.1,"delay":5.2,"duration":1.4,
   "envelope":{"attack":0.02,"decay":0.6,"sustain":0.0,"release":0.7}},
  {"waveform":"triangle","frequency":587.33,"amplitude":0.09,"delay":6.2,"duration":1.6,
   "envelope":{"attack":0.02,"decay":0.6,"sustain":0.0,"release":0.9}}
]' '"space":0.45,"normaliseTo":0.55'

synth music_shiftcalm 8.0 '[
  {"waveform":"sine","frequency":196.00,"amplitude":0.22,"delay":0.0,"duration":2.0,
   "envelope":{"attack":0.02,"decay":0.6,"sustain":0.2,"release":0.6},"lowPassHz":900},
  {"waveform":"sine","frequency":196.00,"amplitude":0.22,"delay":2.0,"duration":2.0,
   "envelope":{"attack":0.02,"decay":0.6,"sustain":0.2,"release":0.6},"lowPassHz":900},
  {"waveform":"sine","frequency":220.00,"amplitude":0.22,"delay":4.0,"duration":2.0,
   "envelope":{"attack":0.02,"decay":0.6,"sustain":0.2,"release":0.6},"lowPassHz":900},
  {"waveform":"sine","frequency":174.61,"amplitude":0.22,"delay":6.0,"duration":2.0,
   "envelope":{"attack":0.02,"decay":0.6,"sustain":0.2,"release":0.6},"lowPassHz":900},
  {"waveform":"triangle","frequency":523.25,"amplitude":0.11,"delay":0.5,"duration":0.9,
   "envelope":{"attack":0.01,"decay":0.35,"sustain":0.0,"release":0.4}},
  {"waveform":"triangle","frequency":587.33,"amplitude":0.1,"delay":1.5,"duration":0.9,
   "envelope":{"attack":0.01,"decay":0.35,"sustain":0.0,"release":0.4}},
  {"waveform":"triangle","frequency":659.25,"amplitude":0.11,"delay":2.5,"duration":0.9,
   "envelope":{"attack":0.01,"decay":0.35,"sustain":0.0,"release":0.4}},
  {"waveform":"triangle","frequency":587.33,"amplitude":0.1,"delay":3.5,"duration":0.9,
   "envelope":{"attack":0.01,"decay":0.35,"sustain":0.0,"release":0.4}},
  {"waveform":"triangle","frequency":523.25,"amplitude":0.11,"delay":4.5,"duration":0.9,
   "envelope":{"attack":0.01,"decay":0.35,"sustain":0.0,"release":0.4}},
  {"waveform":"triangle","frequency":493.88,"amplitude":0.1,"delay":5.5,"duration":0.9,
   "envelope":{"attack":0.01,"decay":0.35,"sustain":0.0,"release":0.4}},
  {"waveform":"triangle","frequency":440.00,"amplitude":0.11,"delay":6.5,"duration":1.4,
   "envelope":{"attack":0.01,"decay":0.5,"sustain":0.0,"release":0.6}}
]' '"space":0.35,"normaliseTo":0.6'

synth music_shiftrush 8.0 '[
  {"waveform":"triangle","frequency":146.83,"amplitude":0.26,"delay":0.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":146.83,"amplitude":0.26,"delay":1.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":164.81,"amplitude":0.26,"delay":2.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":174.61,"amplitude":0.26,"delay":3.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":146.83,"amplitude":0.26,"delay":4.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":196.00,"amplitude":0.26,"delay":5.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":174.61,"amplitude":0.26,"delay":6.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"triangle","frequency":164.81,"amplitude":0.26,"delay":7.0,"duration":1.0,
   "envelope":{"attack":0.01,"decay":0.3,"sustain":0.1,"release":0.3},"lowPassHz":800},
  {"waveform":"square","frequency":659.25,"amplitude":0.07,"delay":0.25,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":783.99,"amplitude":0.07,"delay":0.75,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":880.00,"amplitude":0.07,"delay":1.25,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":783.99,"amplitude":0.07,"delay":1.75,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":659.25,"amplitude":0.07,"delay":2.25,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":587.33,"amplitude":0.07,"delay":4.25,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":659.25,"amplitude":0.07,"delay":5.25,"duration":0.45,
   "envelope":{"attack":0.005,"decay":0.18,"sustain":0.0,"release":0.2},"lowPassHz":2600},
  {"waveform":"square","frequency":880.00,"amplitude":0.08,"delay":6.25,"duration":0.9,
   "envelope":{"attack":0.005,"decay":0.35,"sustain":0.0,"release":0.4},"lowPassHz":2600}
]' '"space":0.3,"normaliseTo":0.65'

echo
echo "Escrito em $OUT"
