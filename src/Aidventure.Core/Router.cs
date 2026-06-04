namespace Aidventure.Core;

/// <summary>
/// Hard-coded scene graph matching SKILL.md routing map.
/// Returns the set of valid routes for a given scene ID.
/// </summary>
public static class Router
{
    private static readonly Dictionary<string, List<SceneRoute>> _routes = new()
    {
        ["S1"] =
        [
            new(RouteKey.A, "S3", "leave quietly via fire escape / back door / avoid lobby / go straight out"),
            new(RouteKey.B, "S2", "go through the lobby / take the elevator / walk downstairs normally"),
            new(RouteKey.C, "S3", "look out the window first / peek outside before leaving", SetSpotted: true)
        ],
        ["S2"] =
        [
            new(RouteKey.A, "S4", "push past them / bluff / walk past / act normal / keep moving forward"),
            new(RouteKey.B, "S3", "back off / retreat / turn around / find another way / go to kitchen / side exit"),
            new(RouteKey.C, "DEAD-1", "hesitate / freeze / do nothing / confront them / talk to them directly")
        ],
        ["S3"] =
        [
            new(RouteKey.A, "S5", "go left / market / crowds / busy street / faster route / shorter"),
            new(RouteKey.B, "S6", "go right / river / riverside / quieter / longer route / open road")
        ],
        ["S4"] =
        [
            new(RouteKey.A, "S5", "market / crowd / lose them in the crowd / go to the stalls"),
            new(RouteKey.B, "S7", "motorcycle / bike / take the keys / ride away"),
            new(RouteKey.C, "DEAD-2", "stop / reason / talk / explain / negotiate with the men")
        ],
        ["S5"] =
        [
            new(RouteKey.A, "S8", "face the hunter / turn around / confront / stop and talk"),
            new(RouteKey.B, "S9", "evade / double back / hide / lose them / slip away quietly"),
            new(RouteKey.C, "S10", "run / sprint / flee / bolt / keep running straight ahead")
        ],
        ["S6"] =
        [
            new(RouteKey.A, "S11", "keep moving / ignore / walk past / don't stop / continue to airport"),
            new(RouteKey.B, "S8", "stop / listen / talk to them / hear what they say / approach the hunter")
        ],
        ["S7"] =
        [
            new(RouteKey.A, "S11", "ditch the bike / abandon it / get off and run on foot"),
            new(RouteKey.B, "DEAD-3", "push through / run the roadblock / keep riding / don't stop")
        ],
        ["S8"] =
        [
            new(RouteKey.A, "S11", "trust them / yes / work together / go with the hunter", SetHunterAlly: "true"),
            new(RouteKey.B, "S9", "refuse / no / go alone / walk away / don't trust them")
        ],
        ["S9"] =
        [
            new(RouteKey.A, "S11", "press on / go forward / freight gate / keep moving / ignore what's behind"),
            new(RouteKey.B, "S8", "stop / wait / face them / turn around / deal with what's behind you")
        ],
        ["S10"] =
        [
            new(RouteKey.A, "S11", "calm down / explain / show ID / cooperate / talk to the officer", BurnMinutes: 12),
            new(RouteKey.B, "DEAD-4", "run / bolt / keep going / ignore the officer / flee")
        ],
        ["S11"] =
        [
            new(RouteKey.A, "S12", "pay / give him the money / hand over the cash"),
            new(RouteKey.B, "S12", "bluff / show documents / talk your way through / use ID / negotiate"),
            new(RouteKey.C, "S13", "find another way / hole in the fence / go around / refuse to pay", SetInjured: true)
        ],
        ["S12"] =
        [
            new(RouteKey.A, "S14", "blend in / walk calmly / keep head down / stay quiet / be inconspicuous", SetS12Method: "blend"),
            new(RouteKey.B, "S14", "distraction / fire alarm / cause a scene / create chaos / make noise", SetS12Method: "distraction"),
            new(RouteKey.C, "S14", "security / get help / report being followed / find a guard", SetS12Method: "escort")
        ],
        ["S13"] =
        [
            new(RouteKey.A, "S14", "run / go / Gate 12 / move fast / keep going / push through"),
            new(RouteKey.B, "DEAD-5", "clean up / first aid / find a bathroom / fix the arm / stop the bleeding")
        ],
        ["S14"] =
        [
            new(RouteKey.A, "END-BEST", "split up: hunter takes the men, you take the boss (requires hunter ally)"),
            new(RouteKey.B, "END-GOOD", "together / all in / both of you at once (requires hunter ally)"),
            new(RouteKey.C, "END-NEUTRAL", "negotiate / talk / offer something / make a deal (alone)"),
            new(RouteKey.D, "END-HARD", "fight / push through / physical confrontation (alone)"),
        ]
    };

    public static List<SceneRoute>? GetRoutes(string sceneId) =>
        _routes.TryGetValue(sceneId, out var r) ? r : null;

    public static bool IsTerminal(string sceneId) =>
        sceneId.StartsWith("DEAD") || sceneId.StartsWith("END");
}
