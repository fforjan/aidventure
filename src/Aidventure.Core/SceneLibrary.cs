namespace Aidventure.Core;

/// <summary>
/// Scene text for all 14 scenes, 6 dead ends, and 4 endings.
/// Used by OllamaService as the authoritative scene content for narration.
/// </summary>
public static class SceneLibrary
{
    public static readonly Dictionary<string, Scene> Scenes = new()
    {
        ["S1"] = new("S1", """
            Hotel room. 3:47 AM. Neon bleeds through the blinds.

            Your phone buzzes — encrypted message, unknown sender:
            "GET OUT NOW. AIRPORT. FLIGHT 7:15 AM. GATE 12. DO NOT STOP."

            You have 3 hours 28 minutes. You don't know who sent this. But your gut says trust it.
            Your bag is half-packed on the chair. The room is quiet. The corridor outside is silent.
            """),

        ["S2"] = new("S2", """
            Ground floor. Two men in armchairs. Shoes too clean. Eyes too sharp. Mafia.

            They clock you the moment the elevator opens. One reaches for his phone.
            You have about three seconds before they're both on their feet.
            """),

        ["S3"] = new("S3", """
            Back alley. Pre-dawn fog. The city holds its breath.

            You're out. No one followed you here. The faint glow of the airport tower is visible above
            the rooftops — maybe 6 kilometres away. The air is cold and damp.

            To the left, the noise of an early morning market is already building — stalls, crowds, cover.
            To the right, the river road stretches quiet and open through the fog.
            """),

        ["S4"] = new("S4", """
            Front street. One of the men is already out the door behind you, talking into his collar.

            You have a 20-second lead and a street full of options. A motorcycle sits at the kerb,
            keys in the ignition. The morning market entrance is half a block ahead.
            And the men behind you are not alone.
            """),

        ["S5"] = new("S5", """
            Spice stalls. Diesel fumes. Vendors shouting. Dawn breaking orange.

            You're threading through the crowd when you feel it — a presence. Then you see them:
            lean, dressed in black, moving with surgical calm through the stalls. Not mafia.
            Not police. Something else entirely. A city hunter. They haven't made a move — yet.
            But their eyes are locked on you.
            """),

        ["S6"] = new("S6", """
            Fog on the water. No one here but fishermen and ghosts.

            The longer road. Quiet. You can think. Halfway along the embankment you notice a figure
            sitting on a bollard — watching the water. They turn as you approach.
            The city hunter. They weren't chasing you. They were waiting for you.

            They don't move. Just watch.
            """),

        ["S7"] = new("S7", """
            The engine screams. Wind. Speed. Freedom — for about four minutes.

            Then the police scanner picks up the plate. A patrol car lights up two blocks ahead,
            pulling across the road. They're setting a roadblock. Blue and white. Nowhere to go around it.
            """),

        ["S8"] = new("S8", """
            They don't reach for a weapon. They hold their hands open.

            "I'm not here to stop you," they say. Their voice is flat, professional.
            "We got the same message. I'm here to make sure you get on that plane."

            You don't know if this is true. But they haven't moved against you.
            And they've had multiple chances.
            """),

        ["S9"] = new("S9", """
            You've shaken them. Or so you think.

            The market thins out. Industrial streets now. The airport district is close —
            you can smell the jet fuel. You glance back. Nothing. But the feeling doesn't leave.

            You reach a junction. Ahead: the freight gate. Behind you: silence that feels too deliberate.
            """),

        ["S10"] = new("S10", """
            A uniform steps out in front of you. Hand raised.

            The knocked-over stall triggered a call. He's young, hand resting on his belt,
            not sure what he's dealing with yet. He just knows there was a disturbance and you were running.
            """),

        ["S11"] = new("S11", """
            Red gate. South side of the airport perimeter. A bored officer in a booth.

            He steps out, looks you up and down with the professional apathy of a man who has seen
            everything and cares about none of it. He holds out his palm.

            "Two hundred. American."
            """),

        ["S12"] = new("S12", """
            Arrivals hall bleeds into departures. Mafia men at every gate entrance, watching faces.

            Gate 12 is at the far end — 400 metres of open floor with nowhere to hide.
            Your flight boards in 18 minutes. The men don't know exactly where you are yet.
            But they will.
            """),

        ["S13"] = new("S13", """
            Torn wire. Mud. The perimeter alarm doesn't trigger — you got lucky.

            Your arm catches on the wire. You feel the warmth of blood before the pain.
            Your jacket is ruined. A maintenance worker twenty metres away is staring at you, frozen.
            You hold eye contact for one second, then keep moving. He doesn't call out.

            You're in. But you're bleeding, and the gate closes in 10 minutes.
            """),

        ["S14"] = new("S14", """
            The jetway door is open. The gate agent has one hand on the handle.

            The mafia boss steps from the crowd. Not a thug — the boss. Expensive coat, cold eyes,
            two men flanking him. He moves like a man who has never been told no and never expected to be.

            "You're not leaving," he says simply.
            """),

        // Dead ends
        ["DEAD-1"] = new("DEAD-1", """
            You hesitate too long. The men are on their feet before you can move.
            The larger one grabs your arm. The other is already on the phone.
            You never make it out of the lobby.
            """, IsDead: true, DeadLastSafe: "S2"),

        ["DEAD-2"] = new("DEAD-2", """
            You stop. You talk. You explain. They don't care.
            A van pulls up. The door opens. That's the last street you see.
            """, IsDead: true, DeadLastSafe: "S4"),

        ["DEAD-3"] = new("DEAD-3", """
            The patrol car doesn't move. You don't slow down.
            A spike strip hidden twenty metres past the roadblock ends it cleanly.
            They find the bike in a ditch. You're already somewhere colder.
            """, IsDead: true, DeadLastSafe: "S7"),

        ["DEAD-4"] = new("DEAD-4", """
            You run. He radios it in. Two more units are waiting at the corner.
            They have you against a wall before you reach the next block.
            The airport tower blinks in the distance, indifferent.
            """, IsDead: true, DeadLastSafe: "S10"),

        ["DEAD-5"] = new("DEAD-5", """
            You find a bathroom. You're thorough. You're careful.
            When you reach Gate 12, the jetway door is sealed.
            The gate agent looks at you. Looks at the clock. Looks away.
            Your flight is a white smear disappearing into the sky.
            """, IsDead: true, DeadLastSafe: "S13"),

        ["DEAD-6"] = new("DEAD-6", """
            The gate agent panics. The boss doesn't.
            He has a word with airport security that takes thirty seconds.
            You spend the next six hours in a holding room with no windows and no phone.
            Flight 7:15 is long gone. So is your leverage.
            """, IsDead: true, DeadLastSafe: "S14"),

        // Endings
        ["END-BEST"] = new("END-BEST", """
            BEST ENDING — Ghost Protocol

            The hunter moves. You move. It's over in seconds — professional, quiet.
            The boss looks at two unconscious men and then at you. He steps aside.
            You walk through the gate without looking back. The door closes.
            You never learn who sent the message — and that's exactly how they wanted it.
            """, IsEnding: true),

        ["END-GOOD"] = new("END-GOOD", """
            GOOD ENDING — Clean Hands

            The hunter handles the men. You square off with the boss.
            One sentence: "Everyone here can see your face."
            He weighs it. Steps aside. You make it. Barely.
            """, IsEnding: true),

        ["END-NEUTRAL"] = new("END-NEUTRAL", """
            NEUTRAL ENDING — The Price of a Ticket

            You give him something you shouldn't — a name, a location, a secret you've been carrying.
            He lets you go. You sit in seat 24F and stare at your hands for the entire flight.
            You made it. But someone else won't.
            """, IsEnding: true),

        ["END-HARD"] = new("END-HARD", """
            HARD ENDING — Scar Tissue

            Three against one. You take hits. You give more. The gate agent is screaming.
            Security is coming. You throw yourself through the jetway door as it's closing
            and collapse into the seat. Your hands are shaking. The seatbelt sign comes on.
            """, IsEnding: true),
    };
}
