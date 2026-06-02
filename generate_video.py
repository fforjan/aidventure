from PIL import Image, ImageDraw, ImageFont
import imageio
import numpy as np

WIDTH, HEIGHT = 1280, 720
FPS = 24
MARGIN_X = 100
LINE_H = 34
BG = (13, 13, 13)
BG_NP = np.array(BG, dtype=np.uint8)

C_HEADER  = (255, 215, 0)
C_SECTION = (255, 255, 255)
C_GM      = (240, 185, 55)
C_PLAYER  = (64, 196, 255)
C_STAGE   = (130, 130, 130)
C_DEAD    = (255, 70, 70)
C_EPITAPH = (180, 180, 180)
C_HR      = (50, 50, 50)

# Typing speed: frames per character (2 = 12 chars/sec at 24fps)
FRAMES_PER_CHAR = 2
# Blink period in frames (6 on / 6 off = 4 Hz)
BLINK_HALF = 6

def font(name, size):
    for n in [name, name.lower(), name.upper()]:
        try:
            return ImageFont.truetype(f"C:/Windows/Fonts/{n}.ttf", size)
        except:
            pass
    return ImageFont.load_default()

F_TITLE   = font("consolab", 34)
F_SECTION = font("consolab", 22)
F_GM      = font("consola",  19)
F_PLAYER  = font("consolab", 19)
F_STAGE   = font("consolai", 17)
F_EPITAPH = font("consolai", 21)

_TMP = ImageDraw.Draw(Image.new("RGB", (1, 1)))

def tw(text, f):
    bx = _TMP.textbbox((0, 0), text, font=f)
    return bx[2] - bx[0]

def wrap(text, f, max_w):
    if not text:
        return []
    words = text.split()
    lines, cur = [], []
    for w in words:
        test = " ".join(cur + [w])
        if tw(test, f) > max_w and cur:
            lines.append(" ".join(cur)); cur = [w]
        else:
            cur.append(w)
    if cur:
        lines.append(" ".join(cur))
    return lines

# ── content ───────────────────────────────────────────────────────────────────
CONTENT = [
    ("header",  "THE LAST FLIGHT"),
    ("sub",     "Session Log"),
    ("spacer",  30),
    ("hr",      ""),
    ("spacer",  20),

    ("section", "WAKE UP"),
    ("spacer",  10),
    ("gm", "The room is dark. Neon from the sign across the street cuts red stripes through the blinds. The AC rattles. Somewhere below, the city breathes."),
    ("gm", "3:47 AM."),
    ("gm", "One message. No sender."),
    ("gm", '"GET OUT NOW. AIRPORT. FLIGHT 7:15 AM. GATE 12. DO NOT STOP."'),
    ("spacer", 14),
    ("player", "Get the bag and jump through the window."),
    ("stage",  "You're on the fourth floor. The window stays closed. You take the fire escape instead."),
    ("spacer",  24),
    ("hr",      ""),
    ("spacer",  20),

    ("section", "CLEAN STREET"),
    ("spacer",  10),
    ("gm", "Four flights of cold iron. Diesel, wet stone, something frying nearby. Pre-dawn fog sits low. The airport tower blinks its slow red pulse to the east. Six kilometres."),
    ("gm", "To the left — market noise, crowds, cover. To the right — the river road, quiet and dark."),
    ("spacer", 14),
    ("player", "Is there any taxi?"),
    ("stage",  "The alley is empty. No cars, no cabs — just damp cobblestones and a stray cat."),
    ("spacer", 10),
    ("player", "I go to the market to be lost in the crowd."),
    ("spacer",  24),
    ("hr",      ""),
    ("spacer",  20),

    ("section", "MORNING MARKET"),
    ("spacer",  10),
    ("gm", "Cardamom, diesel fumes, raw fish on ice. Vendors shout across stalls. Good cover."),
    ("gm", "Then — a figure in black. Moving through the crowd with a stillness that doesn't belong. No browsing, no hesitation. Every step deliberate. Their eyes find yours across thirty metres and hold. Matching your pace exactly."),
    ("spacer", 14),
    ("player", "Start to run and try to jump into a boat."),
    ("stage",  "The river is three kilometres back. You run — knocking a tower of spice jars across the stone. The vendor screams. The figure in black doesn't run. They simply disappear."),
    ("spacer",  24),
    ("hr",      ""),
    ("spacer",  20),

    ("section", "POLICE TANGLE"),
    ("spacer",  10),
    ("gm", "Open street. Lungs burning. A police uniform steps directly into your path."),
    ("gm", "He's young. Radio crackling. He looks at the chaos behind you, then at you."),
    ("gm", '"Stop. Identification."'),
    ("spacer", 14),
    ("player", "Run opposite the cop."),
    ("spacer",  24),
    ("hr",      ""),
    ("spacer",  20),

    ("dead",    "[ DEAD END ]"),
    ("spacer",  14),
    ("gm", "Two more uniforms step out from a side street ahead — already coming for the market disturbance. Nowhere to go. A hand closes on your collar. The radio crackles. Someone reads out a description that sounds a lot like you."),
    ("gm", "The airport tower blinks in the distance. Gate 12. Flight 7:15 AM."),
    ("gm", "Unreachable."),
    ("spacer",  28),
    ("hr",      ""),
    ("spacer",  20),
    ("epitaph", '"Left a trail a mile wide. The city didn\'t even have to try."'),
    ("spacer",  80),
]

# ── render background (player text areas left blank) ─────────────────────────
def render_bg():
    mw = WIDTH - 2 * MARGIN_X
    # first pass: measure height
    y = MARGIN_X
    for t, txt in CONTENT:
        if t == "spacer":  y += int(txt)
        elif t == "hr":    y += 4
        elif t == "header": y += 50
        elif t == "sub":    y += 36
        elif t in ("section", "dead"): y += 38
        elif t == "epitaph": y += len(wrap(txt, F_EPITAPH, mw)) * 36
        elif t == "player":  y += len(wrap(txt, F_PLAYER, mw)) * LINE_H + 8
        elif t == "stage":   y += len(wrap(txt, F_STAGE,  mw)) * LINE_H
        else:                y += len(wrap(txt, F_GM,     mw)) * LINE_H
    total = max(y + MARGIN_X, HEIGHT)

    img = Image.new("RGB", (WIDTH, total), BG)
    d   = ImageDraw.Draw(img)
    y   = MARGIN_X
    player_events = []   # {abs_y, raw, lines, block_h}

    for t, txt in CONTENT:
        if t == "spacer": y += int(txt); continue
        if t == "hr":
            d.line([(MARGIN_X, y), (WIDTH - MARGIN_X, y)], fill=C_HR, width=1)
            y += 4; continue

        if t == "header":
            d.text((MARGIN_X, y), txt, font=F_TITLE,   fill=C_HEADER);  y += 50
        elif t == "sub":
            d.text((MARGIN_X, y), txt, font=F_SECTION, fill=(150,150,150)); y += 36
        elif t == "section":
            d.text((MARGIN_X, y), txt, font=F_SECTION, fill=C_SECTION); y += 38
        elif t == "dead":
            d.text((MARGIN_X, y), txt, font=F_SECTION, fill=C_DEAD);    y += 38
        elif t == "gm":
            for ln in wrap(txt, F_GM, mw):
                d.text((MARGIN_X, y), ln, font=F_GM, fill=C_GM); y += LINE_H
        elif t == "player":
            wl = wrap(txt, F_PLAYER, mw)
            bh = len(wl) * LINE_H
            # draw cyan left bar only — text is blank (typed live)
            d.rectangle([(MARGIN_X - 12, y), (MARGIN_X - 8, y + bh)], fill=C_PLAYER)
            player_events.append({"abs_y": y, "raw": txt, "lines": wl, "block_h": bh})
            y += bh + 8
        elif t == "stage":
            for ln in wrap(txt, F_STAGE, mw):
                d.text((MARGIN_X + 18, y), ln, font=F_STAGE, fill=C_STAGE); y += LINE_H
        elif t == "epitaph":
            for ln in wrap(txt, F_EPITAPH, mw):
                x = (WIDTH - tw(ln, F_EPITAPH)) // 2
                d.text((x, y), ln, font=F_EPITAPH, fill=C_EPITAPH); y += 36

    return img, total, player_events

# ── composite a frame ─────────────────────────────────────────────────────────
def make_frame(bg_arr, scroll_y, typed_done, typing_now=None):
    """
    bg_arr    : full background as numpy array
    scroll_y  : current vertical scroll offset
    typed_done: list of (abs_y, full_lines) — already-typed player lines
    typing_now: (abs_y, partial_lines, show_cursor) — line currently being typed
    """
    yi  = int(scroll_y)
    raw = bg_arr[yi: yi + HEIGHT, 0:WIDTH].copy()
    if raw.shape[0] < HEIGHT:
        pad = np.full((HEIGHT - raw.shape[0], WIDTH, 3), BG, dtype=np.uint8)
        raw = np.vstack([raw, pad])

    overlays = [(ay, ls, False) for ay, ls in typed_done]
    if typing_now:
        overlays.append(typing_now)

    if not overlays:
        return raw

    pil = Image.fromarray(raw)
    d   = ImageDraw.Draw(pil)
    for abs_y, lines, show_cursor in overlays:
        vy = abs_y - yi
        cy = vy
        for ln in lines:
            if -LINE_H < cy < HEIGHT + LINE_H:
                d.text((MARGIN_X, cy), ln, font=F_PLAYER, fill=C_PLAYER)
            cy += LINE_H
        # cursor: thin blinking rectangle after last char
        if show_cursor and lines:
            last = lines[-1]
            cx   = MARGIN_X + tw(last, F_PLAYER) + 3
            cur_top = vy + (len(lines) - 1) * LINE_H + 4
            cur_bot = cur_top + LINE_H - 8
            if 0 <= cur_top < HEIGHT:
                d.rectangle([(cx, cur_top), (cx + 10, cur_bot)], fill=C_PLAYER)
    return np.array(pil)

# ── build all frames ──────────────────────────────────────────────────────────
def build_frames(bg_img, total_height, player_events):
    arr        = np.array(bg_img)
    max_scroll = max(0, total_height - HEIGHT)
    scroll_speed = 55.0 / FPS   # px per frame
    frames     = []
    typed_done = []              # (abs_y, full_lines) already finished

    def f(scroll, typing_now=None):
        return make_frame(arr, scroll, typed_done, typing_now)

    # opening hold
    for _ in range(int(2 * FPS)):
        frames.append(f(0))

    cur = 0.0
    pe_idx = 0

    while True:
        if pe_idx < len(player_events):
            pe = player_events[pe_idx]
            # scroll target: player line sits at ~38% from top
            target = max(0.0, min(float(pe["abs_y"]) - HEIGHT * 0.38, float(max_scroll)))

            # ── scroll to the typing position ────────────────────────────────
            while cur < target:
                cur = min(cur + scroll_speed, target)
                frames.append(f(cur))

            # ── brief pre-type pause (cursor blink, empty) ───────────────────
            for i in range(int(0.5 * FPS)):
                show = (i // BLINK_HALF) % 2 == 0
                frames.append(f(cur, (pe["abs_y"], [], show)))

            # ── typewriter: reveal one char at a time ────────────────────────
            raw_text = pe["raw"]
            frame_count = 0
            for n_chars in range(1, len(raw_text) + 1):
                partial       = raw_text[:n_chars]
                partial_lines = wrap(partial, F_PLAYER, WIDTH - 2 * MARGIN_X)
                show_cursor   = True
                for _ in range(FRAMES_PER_CHAR):
                    frames.append(f(cur, (pe["abs_y"], partial_lines, show_cursor)))
                    frame_count += 1

            # ── post-type: blink cursor on full text, then hold ──────────────
            full_lines = pe["lines"]
            for i in range(int(1.2 * FPS)):
                show = (i // BLINK_HALF) % 2 == 0
                frames.append(f(cur, (pe["abs_y"], full_lines, show)))
            # final hold — cursor off
            for _ in range(int(0.4 * FPS)):
                frames.append(f(cur, (pe["abs_y"], full_lines, False)))

            # mark as done so it stays rendered while we scroll on
            typed_done.append((pe["abs_y"], full_lines))
            pe_idx += 1

        else:
            # no more player events — scroll to end
            if cur >= max_scroll:
                break
            cur = min(cur + scroll_speed, max_scroll)
            frames.append(f(cur))

    # end hold
    for _ in range(int(4 * FPS)):
        frames.append(f(max_scroll))

    print(f"  {len(frames)} frames — {len(frames)/FPS:.1f}s")
    return frames

# ── main ──────────────────────────────────────────────────────────────────────
print("Rendering background image...")
bg_img, total_height, player_events = render_bg()
print(f"  Canvas: {WIDTH}x{total_height}px  |  {len(player_events)} player line(s) to type")

print("Building frames...")
frames = build_frames(bg_img, total_height, player_events)

out = r"C:\Users\ISYS36005\source\repos\Aidventure\session_replay.mp4"
print(f"Writing {out} ...")
writer = imageio.get_writer(out, fps=FPS, quality=8, macro_block_size=1)
for i, frm in enumerate(frames):
    writer.append_data(frm)
    if i % (FPS * 5) == 0:
        print(f"  {i}/{len(frames)} frames...")
writer.close()
print(f"Done! -> {out}")
