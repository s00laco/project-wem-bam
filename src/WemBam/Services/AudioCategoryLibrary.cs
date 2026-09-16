using System;
using System.Collections.Generic;
using System.Linq;
using WemBam.Models;

namespace WemBam.Services
{
    public static class AudioCategoryLibrary
    {
        private static readonly IReadOnlyDictionary<string, string> Translations =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["activate"] = "Activate",
                ["activator"] = "Activator",
                ["add"] = "add (Additives)",
                ["additives"] = "Additives",
                ["Affl"] = "affl (Affliction)",
                ["afterburner"] = "Afterburner",
                ["aid"] = "Aid",
                ["ak"] = "ak (Akila)",
                ["akila"] = "Akila",
                ["alarm"] = "Alarm",
                ["alert"] = "Alert",
                ["allied"] = "Allied",
                ["amb"] = "amb (Ambient)",
                ["ammo"] = "Ammunition",
                ["animation"] = "Animation",
                ["archipelago"] = "Archipelago",
                ["armor"] = "Armor",
                ["articulation"] = "Articulation",
                ["artifact"] = "Artifact",
                ["atlantis"] = "Atlantis",
                ["atmos"] = "admos (Atmosphere)",
                ["attack"] = "Attack",
                ["audio"] = "Audio",
                ["auto"] = "Automatic",
                ["ball"] = "Ballistic",
                ["ballistic"] = "Ballistic",
                ["bash"] = "Bash",
                ["beep"] = "Beep",
                ["big"] = "Big",
                ["binary"] = "Binary",
                ["biome"] = "Biome",
                ["biomes"] = "Biomes",
                ["biped"] = "Biped",
                ["birds"] = "Birds",
                ["blast"] = "Blast",
                ["body"] = "Body",
                ["bolt"] = "Bolt",
                ["bot"] = "Robot",
                ["box"] = "Box",
                ["break"] = "Break",
                ["breath"] = "Breath",
                ["broken"] = "Broken",
                ["builder"] = "Builder",
                ["builders"] = "Builders",
                ["bullet"] = "Bullet",
                ["buzz"] = "Buzz",
                ["_by_"] = "_by_ (projectile wizz-by)",
                ["bys"] = "bys (Pass-by)",
                ["canyon"] = "Canyon",
                ["casings"] = "Casings",
                ["catwalk"] = "Catwalk",
                ["cave"] = "Cave",
                ["charge"] = "Charge",
                ["cities"] = "Cities",
                ["city"] = "City",
                ["clean"] = "Clean",
                ["close"] = "Close",
                ["cockpit"] = "Cockpit",
                ["combatech"] = "Combatech (Manufacturer)",
                ["computer"] = "Computer",
                ["computers"] = "Computers",
                ["coniferous"] = "Coniferous",
                ["conscious"] = "Conscious",
                ["container"] = "Container",
                ["control"] = "Control",
                ["controls"] = "Controls",
                ["cracking"] = "Cracking",
                ["crafting"] = "Crafting",
                ["creak"] = "Creak",
                ["creaking"] = "Creaking",
                ["creaks"] = "Creaks",
                ["creature"] = "Creature",
                ["cricket"] = "Cricket",
                ["critical"] = "Critical",
                ["critter"] = "Critter",
                ["crowd"] = "Crowd",
                ["cup"] = "Cup",
                ["cydonia"] = "Cydonia",
                ["day"] = "Day",
                ["death"] = "Death",
                ["debris"] = "Debris",
                ["deciduous"] = "Deciduous",
                ["deep"] = "Deep",
                ["default"] = "Default",
                ["deimos"] = "Deimos (Manufacturer)",
                ["deploy"] = "Deploy",
                ["desert"] = "Desert",
                ["destruction"] = "Destruction",
                ["diegetic"] = "Diegetic",
                ["distant"] = "Distant",
                ["dome"] = "Dome",
                ["door"] = "Door",
                ["dr"] = "dr (Door)",
                ["drive"] = "Drive",
                ["Drs"] = "Drs (Doors)",
                ["dungeon"] = "Dungeon",
                ["ease"] = "Ease",
                ["electrical"] = "Electrical",
                ["elevator"] = "Elevator",
                ["embassy"] = "Embassy",
                ["end"] = "End",
                ["energy"] = "Energy",
                ["engine"] = "Engine",
                ["engines"] = "Engines",
                ["enter"] = "Enter",
                ["Env"] = "Env (Environment)",
                ["equip"] = "Equip",
                ["explore"] = "Explore",
                ["explosion"] = "Explosion",
                ["ext"] = "Ext (Exterior)",
                ["exterior"] = "Exterior",
                ["exteriors"] = "Exteriors",
                ["extras"] = "Extras",
                ["Faction"] = "faction (Music - Military, Freestar)",
                ["factory"] = "Factory",
                ["far"] = "Far",
                ["female"] = "Female",
                ["fence"] = "Fence",
                ["fidget"] = "Fidget",
                ["fire"] = "Fire",
                ["first"] = "First",
                ["flamethrower"] = "Flamethrower",
                ["flesh"] = "Flesh",
                ["floater"] = "Floater",
                ["fly"] = "Fly",
                ["flyBy"] = "flyby (Ship Fly-By)",
                ["flyer"] = "flyer (Flying Creature Sounds)",
                ["foley"] = "Foley",
                ["food"] = "Food",
                ["footstep"] = "Footstep",
                ["force"] = "Force",
                ["forest"] = "Forest",
                ["fracking"] = "Fracking",
                ["frozen"] = "Frozen",
                ["fst"] = "fst (Footsteps)",
                ["furniture"] = "Furniture",
                ["fx"] = "fx (Effects)",
                ["gas"] = "Gas",
                ["gear"] = "Gear",
                ["gen"] = "gen (General/Generic)",
                ["generator"] = "Generator",
                ["generic"] = "Generic",
                ["generics"] = "Generics",
                ["genesis"] = "Genesis",
                ["grass"] = "Grass",
                ["grav"] = "Grav Drive",
                ["gravel"] = "Gravel",
                ["gravity"] = "Gravity",
                ["graze"] = "Graze",
                ["grenades"] = "Grenades",
                ["groan"] = "Groan",
                ["groans"] = "Groans",
                ["gust"] = "Gust",
                ["gusts"] = "Gusts",
                ["hab"] = "hab (Habitat Module)",
                ["harvester"] = "Harvester",
                ["hazard"] = "Hazard",
                ["hazards"] = "Hazards",
                ["heavy"] = "Heavy",
                ["hexapod"] = "Hexapod",
                ["high"] = "High",
                ["hit"] = "Hit",
                ["hollow"] = "Hollow",
                ["hopper"] = "Hopper",
                ["howl"] = "Howl",
                ["hud"] = "HUD",
                ["human"] = "Human",
                ["hunting"] = "Hunting",
                ["ice"] = "Ice",
                ["idle"] = "Idle",
                ["impact"] = "Impact",
                ["impulse"] = "Impulse",
                ["ind"] = "ind (Industrial)",
                ["industrial"] = "Industrial",
                ["infinity"] = "Infinity",
                ["ingredient"] = "Ingredient",
                ["insect"] = "Insect",
                ["insects"] = "Insects",
                ["int"] = "int (Interior)",
                ["intercom"] = "intercom (Background Comms Walla)",
                ["interior"] = "Interior",
                ["interiors"] = "Interiors",
                ["inventory"] = "Inventory",
                ["item"] = "Item",
                ["itm"] = "itm (Item)",
                ["jump"] = "Jump",
                ["kaiser"] = "Kaiser",
                ["key"] = "Key",
                ["kit"] = "Kit",
                ["lab"] = "Lab",
                ["land"] = "Land",
                ["landing"] = "Landing",
                ["large"] = "Large",
                ["larva"] = "Larva",
                ["laser"] = "Laser",
                ["launcher"] = "Launcher",
                ["layer"] = "layer (Layered Audio)",
                ["lc"] = "lc (Location)",
                ["level"] = "Level",
                ["lg"] = "lg (Large)",
                ["light"] = "Light",
                ["lightning"] = "Lightning",
                ["liquid"] = "Liquid",
                ["load"] = "Load",
                ["lock"] = "Lock",
                ["lodge"] = "Lodge",
                ["loop"] = "Loop",
                ["loose"] = "loose (Loosened/Falling Objects)",
                ["_lowBass"] = "_lowBass",
                ["_lowRumble"] = "_lowRumble",
                ["_low"] = "_low (Low Audio)",
                ["lp"] = "Loop (Looped Audio)",
                ["lpm"] = "lp (Looped Audio)",
                ["machine"] = "Machine",
                ["machines"] = "Machines",
                ["male"] = "Male",
                ["manta"] = "manta (Generic Creepy Creature Sounds)",
                ["mantid"] = "mantid (creepy insectoid)",
                ["mantle"] = "mantle (Land after Climbing Over)",
                ["marker"] = "marker (Localised/locally placed Audio)",
                ["markers"] = "markers (Localised/locally placed Audio)",
                ["mechanical"] = "Mechanical",
                ["med"] = "med (Medium)",
                ["medium"] = "Medium",
                ["melee"] = "Melee",
                ["menu"] = "Menu",
                ["mine"] = "mine (Underground caves/mines + explosives)",
                ["missile"] = "Missile",
                ["morning"] = "Morning",
                ["mountains"] = "Mountains",
                ["movement"] = "Movement",
                ["mud"] = "Mud",
                ["mus"] = "mus (Music)",
                ["nasa"] = "nasa (All Things NASA)",
                ["neon"] = "Neon",
                ["night"] = "Night",
                ["nishina"] = "Nishina",
                ["nova"] = "nova (Nova Galactic (Manufacturer))",
                ["npc"] = "NPC",
                ["obj"] = "obj (Object)",
                ["ocean"] = "Ocean",
                ["octopede"] = "Octopede",
                ["off"] = "Off",
                ["office"] = "Office",
                ["oneshot"] = "One-shot",
                ["oneshots"] = "One-shots",
                ["open"] = "Open",
                ["ore"] = "ore (Mining Sounds)",
                ["out"] = "Out",
                ["outpost"] = "Outpost",
                ["pad"] = "Pad",
                ["palette"] = "Palette",
                ["paper"] = "Paper",
                ["part"] = "Part",
                ["particle"] = "Particle",
                ["pc"] = "pc (Player Character)",
                ["person"] = "Person",
                ["phy"] = "phy (Physical)",
                ["pickup"] = "pickup (Picking up)",
                ["pilot"] = "pilot (Pilot Seat - misc sounds)",
                ["pipe"] = "Pipe",
                ["pistol"] = "Pistol",
                ["planet"] = "Planet",
                ["planets"] = "Planets",
                ["player"] = "Player",
                ["powers"] = "powers (Starborn FX)",
                ["projectile"] = "Projectile",
                ["prototyping"] = "Prototyping",
                ["pump"] = "Pump",
                ["putdown"] = "Put-down",
                ["puzzle"] = "Puzzle",
                ["qst"] = "Quest",
                ["quad"] = "Quadruped",
                ["quadruped"] = "Quadruped",
                ["rain"] = "Rain",
                ["rattle"] = "Rattle",
                ["rattles"] = "Rattles",
                ["reladyne"] = "Reladyne (Manufacturer)",
                ["release"] = "Release",
                ["reload"] = "Reload",
                ["research"] = "Research",
                ["reverb"] = "Reverb",
                ["rifle"] = "Rifle",
                ["rings"] = "Rings",
                ["Rm"] = "rm (Room)",
                ["robot"] = "Robot",
                ["rock"] = "Rock",
                ["room"] = "Room",
                ["roomtone"] = "Room Tone",
                ["rotate"] = "Rotate",
                ["rumble"] = "Rumble",
                ["run"] = "Run",
                ["SAE"] = "SAE (Slayton Aeronautics - Manufacturer)",
                ["sand"] = "Sand",
                ["sandstorm"] = "Sandstorm",
                ["scanner"] = "Scanner",
                ["sciLab"] = "sciLab (Science Lab)",
                ["science"] = "Science",
                ["score"] = "Score",
                ["screen"] = "Screen",
                ["securityDoor"] = "SecurityDoor",
                ["securityCamera"] = "SecurityCamera",
                ["minigame_security"] = "minigame_security (Digipick Sounds)",
                ["semi"] = "Semi-automatic",
                ["seq"] = "seq (Sequence)",
                ["sequence"] = "Sequence",
                ["settlement"] = "Settlement",
                ["settlements"] = "Settlements",
                ["sfx"] = "Sound Effects",
                ["shake"] = "Shake",
                ["shield"] = "Shield",
                ["ship"] = "Ship",
                ["ship_third"] = "ship_third (Third Person Ext. Ship)",
                ["short"] = "Short",
                ["shot"] = "Shot",
                ["shotgun"] = "Shotgun",
                ["silenced"] = "Silenced (Weapons)",
                ["sing"] = "Sing (Bird Song)",
                ["skin"] = "Skin",
                ["slayton"] = "Slayton Aeronautics (Manufacturer)",
                ["slide"] = "Slide (Sliding Footsteps)",
                ["sm"] = "sm (Small)",
                ["small"] = "Small",
                ["sneak"] = "Sneak",
                ["sniper"] = "Sniper",
                ["snow"] = "Snow",
                ["soft"] = "Soft",
                ["solid"] = "Solid",
                ["space"] = "Space",
                ["spaceship"] = "Spaceship",
                ["spacesuit"] = "Spacesuit",
                ["sparks"] = "Sparks",
                ["special"] = "Special (Special Quest Music/Starborn)",
                ["spray"] = "Spray",
                ["sprint"] = "Sprint",
                ["star_station"] = "star_station (Star Station Amb Audio)",
                ["starborn"] = "Starborn",
                ["starmap"] = "Starmap",
                ["start"] = "Start",
                ["staryard"] = "Staryard",
                ["staryards"] = "Staryards",
                ["researchStation"] = "researchStation (Research Workbench)",
                ["steam"] = "Steam",
                ["stinger"] = "Stinger (Short Musical Cue)",
                ["stop"] = "Stop",
                ["storm"] = "Storm",
                ["stroud"] = "stroud (Stroud-Eklund - Manufacturer)",
                ["suppressed"] = "Suppressed",
                ["suppressor"] = "Suppressor",
                ["suppressors"] = "Suppressors",
                ["swing"] = "Swing",
                ["switch"] = "Switch",
                ["switches"] = "Switches",
                ["system"] = "System (Ambient Nature Audio Systems)",
                ["taiyo"] = "taiyo (Taiyo Astroneering - Manufacturer)",
                ["takeoff"] = "Takeoff",
                ["taunt"] = "Taunt",
                ["tech"] = "tech",
                ["temple"] = "Temple",
                ["terminal"] = "Terminal",
                ["terminals"] = "Terminals",
                ["terrormorph"] = "Terrormorph",
                ["thunder"] = "Thunder",
                ["tiny"] = "Tiny",
                ["tool"] = "Tool",
                ["tools"] = "Tools",
                ["traps"] = "Traps",
                ["tropical"] = "Tropical",
                ["turret"] = "Turret",
                ["turrets"] = "Turrets",
                ["ui"] = "ui (User Interface)",
                ["unity"] = "Unity",
                ["use"] = "Use",
                ["varuun"] = "varuun",
                ["vasco"] = "Vasco",
                ["veh"] = "veh (Vehicle)",
                ["vent"] = "Vent",
                ["voc"] = "voc (Voice)",
                ["volcanic"] = "Volcanic",
                ["vox"] = "vox (Voice)",
                ["walk"] = "Walk",
                ["walla"] = "walla (Background People Noise)",
                ["water"] = "Water",
                ["Wea"] = "wea (Weapon)",
                ["weapon"] = "Weapon",
                ["weapons"] = "Weapons",
                ["weather"] = "Weather",
                ["welder"] = "Welder",
                ["wind"] = "Wind",
                ["winds"] = "Winds",
                ["wpn"] = "wpn (Weapon)",
                ["xxx"] = "Test Audio"
            };

        public static AudioCategory Create(string value)
        {
            ArgumentNullException.ThrowIfNull(value);

            string trimmedValue = value.Trim();

            if (Translations.TryGetValue(trimmedValue, out string? displayName))
            {
                return new AudioCategory
                {
                    Code = trimmedValue,
                    DisplayName = displayName
                };
            }

            return new AudioCategory
            {
                Code = trimmedValue,
                DisplayName = trimmedValue
            };
        }

        public static IReadOnlyList<AudioCategory> CreateMany(IEnumerable<string> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            return values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(Create)
                .ToList();
        }

        public static IReadOnlyList<AudioCategory> CreateManyFromWwiseText(params string?[] values)
        {
            ArgumentNullException.ThrowIfNull(values);

            var matches = new List<AudioCategory>();

            foreach (string? value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                foreach (KeyValuePair<string, string> translation in Translations)
                {
                    if (!ContainsCompleteKeyword(value, translation.Key))
                    {
                        continue;
                    }

                    matches.Add(new AudioCategory
                    {
                        Code = translation.Key,
                        DisplayName = translation.Value
                    });
                }
            }

            return matches
                .GroupBy(category => category.Code, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(category => category.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool ContainsCompleteKeyword(string value, string keyword)
        {
            int searchStart = 0;

            while (searchStart < value.Length)
            {
                int matchIndex = value.IndexOf(keyword, searchStart, StringComparison.OrdinalIgnoreCase);

                if (matchIndex < 0)
                {
                    return false;
                }

                bool leftBoundary = matchIndex == 0 || !char.IsLetterOrDigit(value[matchIndex - 1]);
                int matchEnd = matchIndex + keyword.Length;
                bool rightBoundary = matchEnd == value.Length || !char.IsLetterOrDigit(value[matchEnd]);

                if (leftBoundary && rightBoundary)
                {
                    return true;
                }

                searchStart = matchIndex + 1;
            }

            return false;
        }
    }
}
