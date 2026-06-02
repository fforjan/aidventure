from PIL import Image, ImageDraw, ImageFont
import imageio
import numpy as np

WIDTH, HEIGHT = 1280, 720
FPS = 24
MARGIN_X = 100
LINE_H = 34
BG = (13, 13, 13)

C_HEADER  = (255, 215, 0)
C_SECTION = (255, 255, 255)
C_GM      = (240, 185, 55)
C_PLAYER  = (64, 196, 255)
C_STAGE   = (130, 130, 130)
C_DEAD    = (255, 70, 70)
C_EPITAPH = (180, 180, 180)
C_HR      = (50, 50, 50)

FRAMES_PER_CHAR = 2   # 12 chars/sec
BLINK_HALF      = 6   # cursor blink: 6 frames on / 6 off
OPEN_REVEAL_PX  = int(LINE_H * 1.5)   # px per frame for opening reveal (slow)
REVEAL_PX       = LINE_H * 3          # px per frame for post-enter reveal (fast)
SCROLL_SPEED    = 55.0 / FPS

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
# (type, text)  — player lines are typed live; everything after them is hidden
# until the player "presses enter"
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

    ("player", "Get the bag and jump through the window."),     # P0

    ("stage",  "You're on the fourth floor. The window stays closed. You take the fire escape instead."),
    ("spacer",  24),
    ("hr",      ""),
    ("spacer",  20),
    ("section", "CLEAN STREET"),
    ("spacer",  10),
    ("gm", "Four flights of cold iron. Diesel, wet stone, something frying nearby. Pre-dawn fog sits low. The airport tower blinks its slow red pulse to the east. Six kilometres."),
    ("gm", "To the left — market noise, crowds, cover. To the right — the river road, quiet and dark."),
    ("spacer", 14),

    ("player", "Is there any taxi?"),                           # P1

    ("stage",  "The alley is empty. No cars, no cabs — just damp cobblestones and a stray cat."),
    ("spacer", 10),

    ("player", "I go to the market to be lost in the crowd."),  # P2

    ("spacer",  24),
    ("hr",      ""),
    ("spacer",  20),
    ("section", "MORNING MARKET"),
    ("spacer",  10),
    ("gm", "Cardamom, diesel fumes, raw fish on ice. Vendors shout across stalls. Good cover."),
    ("gm", "Then — a figure in black. Moving through the crowd with a stillness that doesn't belong. No browsing, no hesitation. Every step deliberate. Their eyes find yours across thirty metres and hold. Matching your pace exactly."),
    ("spacer", 14),

    ("player", "Start to run and try to jump into a boat."),    # P3

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

    ("player", "Run opposite the cop."),                        # P4

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

# ── render background (NO player text — drawn live as overlay) ────────────────
def render_bg():
    mw = WIDTH - 2 * MARGIN_X

    # first pass: total height + record player event positions
    y = MARGIN_X
    player_events = []
    for t, txt in CONTENT:
        if t == "spacer":   y += int(txt)
        elif t == "hr":     y += 4
        elif t == "header": y += 50
        elif t == "sub":    y += 36
        elif t in ("section", "dead"): y += 38
        elif t == "epitaph": y += len(wrap(txt, F_EPITAPH, mw)) * 36
        elif t == "player":
            wl = wrap(txt, F_PLAYER, mw)
            player_events.append({"abs_y": y, "raw": txt, "lines": wl,
                                   "block_h": len(wl) * LINE_H})
            y += len(wl) * LINE_H + 8
        elif t == "stage":  y += len(wrap(txt, F_STAGE,  mw)) * LINE_H
        else:               y += len(wrap(txt, F_GM,     mw)) * LINE_H
    total = max(y + MARGIN_X, HEIGHT)

    # second pass: draw everything EXCEPT player text
    img = Image.new("RGB", (WIDTH, total), BG)
    d   = ImageDraw.Draw(img)
    y   = MARGIN_X
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
            # blank space — drawn live in overlay
            y += len(wrap(txt, F_PLAYER, mw)) * LINE_H + 8
        elif t == "stage":
            for ln in wrap(txt, F_STAGE, mw):
                d.text((MARGIN_X + 18, y), ln, font=F_STAGE, fill=C_STAGE); y += LINE_H
        elif t == "epitaph":
            for ln in wrap(txt, F_EPITAPH, mw):
                x = (WIDTH - tw(ln, F_EPITAPH)) // 2
                d.text((x, y), ln, font=F_EPITAPH, fill=C_EPITAPH); y += 36

    return img, total, player_events

# ── composite a single frame ──────────────────────────────────────────────────
def make_frame(bg_arr, scroll_y, revealed_y, typed_done, typing_now=None,
               enter_flash_pe=None):
    """
    bg_arr        : full background numpy array (narrator text only)
    scroll_y      : current viewport offset
    revealed_y    : bg content below this y is masked (hidden)
    typed_done    : [(abs_y, lines)] — completed player lines (always shown)
    typing_now    : (abs_y, partial_lines, show_cursor) — line being typed
    enter_flash_pe: player_event dict — flash entire line white for enter effect
    """
    yi  = int(scroll_y)
    raw = bg_arr[yi: yi + HEIGHT, 0:WIDTH].copy()
    if raw.shape[0] < HEIGHT:
        pad = np.full((HEIGHT - raw.shape[0], WIDTH, 3), BG, dtype=np.uint8)
        raw = np.vstack([raw, pad])

    # mask narrator content not yet revealed
    mask_start = max(0, int(revealed_y) - yi)
    if mask_start < HEIGHT:
        raw[mask_start:] = BG

    pil = Image.fromarray(raw)
    d   = ImageDraw.Draw(pil)

    # draw completed player lines
    for abs_y, lines in typed_done:
        _draw_player(d, abs_y - yi, lines, C_PLAYER, False)

    # draw currently-typing line
    if typing_now:
        ay, partial, show_cur = typing_now
        _draw_player(d, ay - yi, partial, C_PLAYER, show_cur)

    # enter flash: entire line + bar in white
    if enter_flash_pe:
        pe = enter_flash_pe
        _draw_player(d, pe['abs_y'] - yi, pe['lines'], (255, 255, 255), False,
                     bar_col=(255, 255, 255))

    return np.array(pil)

def _draw_player(d, vy, lines, text_col, show_cursor, bar_col=None):
    if bar_col is None:
        bar_col = C_PLAYER
    bar_h = max(len(lines), 1) * LINE_H
    if -LINE_H < vy < HEIGHT + LINE_H:
        d.rectangle([(MARGIN_X - 12, vy), (MARGIN_X - 8, vy + bar_h)], fill=bar_col)
    cy = vy
    for ln in lines:
        if -LINE_H < cy < HEIGHT + LINE_H:
            d.text((MARGIN_X, cy), ln, font=F_PLAYER, fill=text_col)
        cy += LINE_H
    if show_cursor:
        last  = lines[-1] if lines else ""
        cx    = MARGIN_X + tw(last, F_PLAYER) + 3
        ct    = vy + max(0, len(lines) - 1) * LINE_H + 4
        cb    = ct + LINE_H - 8
        if 0 <= ct < HEIGHT:
            d.rectangle([(cx, ct), (cx + 10, cb)], fill=text_col)

# ── main animation builder ────────────────────────────────────────────────────
def build_frames(bg_img, total_height, player_events):
    arr        = np.array(bg_img)
    max_scroll = max(0, total_height - HEIGHT)
    frames     = []
    typed_done = []

    # revealed_y starts at 0 — we wipe in the opening narrator text first
    revealed_y = 0.0
    cur_scroll = 0.0

    def F(scroll=None, typing_now=None, flash_pe=None):
        s = scroll if scroll is not None else cur_scroll
        return make_frame(arr, s, revealed_y, typed_done, typing_now, flash_pe)

    # ── Phase 0: reveal opening narrator text (G0) wipe-down ─────────────────
    open_target = float(player_events[0]['abs_y']) if player_events else float(total_height)
    while revealed_y < open_target:
        revealed_y = min(revealed_y + OPEN_REVEAL_PX, open_target)
        # gentle scroll to follow reveal
        target_s = max(0.0, min(revealed_y - HEIGHT * 0.78, float(max_scroll)))
        if target_s > cur_scroll:
            cur_scroll = min(cur_scroll + SCROLL_SPEED, target_s)
        frames.append(F())

    # hold on opening scene
    for _ in range(int(2.0 * FPS)):
        frames.append(F())

    # ── Main loop: for each player event ──────────────────────────────────────
    for pe_idx, pe in enumerate(player_events):

        # 1. scroll so the player line sits ~38% from top
        target_s = max(0.0, min(float(pe['abs_y']) - HEIGHT * 0.38, float(max_scroll)))
        while cur_scroll < target_s:
            cur_scroll = min(cur_scroll + SCROLL_SPEED, target_s)
            frames.append(F())

        # 2. pre-type cursor blink on empty line (0.5s)
        for i in range(int(0.5 * FPS)):
            show = (i // BLINK_HALF) % 2 == 0
            frames.append(F(typing_now=(pe['abs_y'], [], show)))

        # 3. typewriter — one char every FRAMES_PER_CHAR frames
        raw = pe['raw']
        mw  = WIDTH - 2 * MARGIN_X
        for n in range(1, len(raw) + 1):
            pl = wrap(raw[:n], F_PLAYER, mw)
            for _ in range(FRAMES_PER_CHAR):
                frames.append(F(typing_now=(pe['abs_y'], pl, True)))

        # 4. post-type blink with full text (1.2s)
        full_lines = pe['lines']
        for i in range(int(1.2 * FPS)):
            show = (i // BLINK_HALF) % 2 == 0
            frames.append(F(typing_now=(pe['abs_y'], full_lines, show)))

        # 5. "Enter" flash — whole line flashes white (5 frames)
        for _ in range(5):
            frames.append(F(flash_pe=pe))

        # mark this player line as permanently typed
        typed_done.append((pe['abs_y'], full_lines))

        # 6. reveal next narrator block with fast wipe-down
        if pe_idx + 1 < len(player_events):
            next_target = float(player_events[pe_idx + 1]['abs_y'])
        else:
            next_target = float(total_height)

        while revealed_y < next_target:
            revealed_y = min(revealed_y + REVEAL_PX, next_target)
            # viewport follows reveal (keeping reveal near bottom 25%)
            target_s = max(0.0, min(revealed_y - HEIGHT * 0.78, float(max_scroll)))
            if target_s > cur_scroll:
                cur_scroll = min(cur_scroll + SCROLL_SPEED * 1.5, target_s)
            frames.append(F())

        # 7. hold after reveal (2s before next player input)
        for _ in range(int(2.0 * FPS)):
            frames.append(F())

    # ── Final hold (4s) ───────────────────────────────────────────────────────
    for _ in range(int(4.0 * FPS)):
        frames.append(F(scroll=float(max_scroll)))

    print(f"  {len(frames)} frames — {len(frames)/FPS:.1f}s")
    return frames

# ── entry point ───────────────────────────────────────────────────────────────
print("Rendering background...")
bg_img, total_height, player_events = render_bg()
print(f"  Canvas {WIDTH}x{total_height}px | {len(player_events)} player lines")

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
