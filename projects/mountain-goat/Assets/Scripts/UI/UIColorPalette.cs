using UnityEngine;

/// <summary>
/// Centralized color palette for all UI in the Mountain Goat game.
/// All UI scripts reference these constants instead of hardcoding colors.
/// </summary>
public static class UIColorPalette
{
    // ── Primary Accent Colors ──
    public static readonly Color PrimaryGreen   = new Color(0.30f, 0.58f, 0.32f, 1f);
    public static readonly Color PrimaryBrown   = new Color(0.45f, 0.35f, 0.22f, 1f);
    public static readonly Color AccentGold     = new Color(0.95f, 0.80f, 0.10f, 1f);

    // ── Panel / Background Colors ──
    public static readonly Color PanelBg       = new Color(0.10f, 0.12f, 0.08f, 0.94f);
    public static readonly Color OverlayDim    = new Color(0f, 0f, 0f, 0.55f);
    public static readonly Color DarkBg        = new Color(0.06f, 0.08f, 0.05f, 0.92f);
    public static readonly Color ScrollBg      = new Color(0.08f, 0.10f, 0.06f, 0.70f);

    // ── Button Colors ──
    public static readonly Color BtnPlay        = new Color(0.30f, 0.60f, 0.28f, 0.95f);
    public static readonly Color BtnBack        = new Color(0.50f, 0.45f, 0.38f, 0.90f);
    public static readonly Color BtnDelete      = new Color(0.72f, 0.25f, 0.20f, 0.90f);
    public static readonly Color BtnConfirm     = new Color(0.32f, 0.62f, 0.30f, 0.95f);
    public static readonly Color BtnCancel      = new Color(0.58f, 0.35f, 0.30f, 0.95f);
    public static readonly Color BtnTutorial    = new Color(0.25f, 0.55f, 0.35f, 0.95f);
    public static readonly Color BtnProfiles    = new Color(0.35f, 0.55f, 0.25f, 0.95f);
    public static readonly Color BtnLeaderboard = new Color(0.25f, 0.45f, 0.60f, 0.95f);
    public static readonly Color BtnQuit        = new Color(0.55f, 0.30f, 0.25f, 0.90f);
    public static readonly Color BtnArrow       = new Color(1f, 1f, 1f, 0.94f);

    // ── Text Colors ──
    public static readonly Color TextPrimary    = Color.white;
    public static readonly Color TextSecondary  = new Color(0.75f, 0.75f, 0.70f, 1f);
    public static readonly Color TextMuted      = new Color(0.55f, 0.55f, 0.50f, 0.85f);
    public static readonly Color TextGold       = new Color(0.95f, 0.85f, 0.10f, 1f);
    public static readonly Color TextDark       = new Color(0.14f, 0.16f, 0.12f, 1f);
    public static readonly Color TextError      = new Color(1f, 0.40f, 0.30f, 1f);
    public static readonly Color TextNewRecord  = new Color(1f, 0.85f, 0.10f, 1f);
    public static readonly Color TextGreen      = new Color(0.30f, 1f, 0.30f, 0.90f);

    // ── Rank Colors ──
    public static readonly Color RankGold       = new Color(0.95f, 0.80f, 0.10f, 1f);
    public static readonly Color RankSilver     = new Color(0.80f, 0.80f, 0.80f, 1f);
    public static readonly Color RankBronze     = new Color(0.80f, 0.55f, 0.25f, 1f);

    // ── HUD Colors ──
    public static readonly Color HungerGreen    = new Color(0.30f, 0.85f, 0.30f, 1f);
    public static readonly Color HungerYellow   = new Color(0.95f, 0.80f, 0.10f, 1f);
    public static readonly Color HungerRed      = new Color(0.95f, 0.25f, 0.15f, 1f);
    public static readonly Color CoinGold       = new Color(0.95f, 0.80f, 0.10f, 1f);

    // ── Slot / Profile Colors ──
    public static readonly Color SlotEmpty      = new Color(0.30f, 0.32f, 0.28f, 0.80f);
    public static readonly Color SlotOccupied   = new Color(0.25f, 0.35f, 0.22f, 0.90f);
    public static readonly Color SlotActive     = new Color(0.30f, 0.50f, 0.22f, 0.95f);

    // ── Leaderboard Entry Colors ──
    public static readonly Color EntryNormal    = new Color(0.22f, 0.25f, 0.20f, 0.85f);
    public static readonly Color EntryHighlight = new Color(0.35f, 0.45f, 0.18f, 0.90f);
    public static readonly Color EntryDeleted   = new Color(0.18f, 0.18f, 0.16f, 0.75f);

    // ── Input Field Colors ──
    public static readonly Color InputBg        = new Color(0.25f, 0.28f, 0.22f, 0.95f);
    public static readonly Color InputBorder    = new Color(0.45f, 0.50f, 0.40f, 0.80f);

    // ── Scrollbar Colors ──
    public static readonly Color ScrollbarTrack = new Color(0.10f, 0.10f, 0.08f, 0.50f);
    public static readonly Color ScrollbarHandle = new Color(0.40f, 0.42f, 0.38f, 0.70f);

    // ── Helper Methods ──

    /// <summary>Return a brighter version of a color for hover state.</summary>
    public static Color HoverVariant(Color c)
    {
        return new Color(
            Mathf.Min(1f, c.r * 1.25f),
            Mathf.Min(1f, c.g * 1.25f),
            Mathf.Min(1f, c.b * 1.25f),
            c.a);
    }

    /// <summary>Return a darker version of a color for press state.</summary>
    public static Color PressVariant(Color c)
    {
        return new Color(
            c.r * 0.80f,
            c.g * 0.80f,
            c.b * 0.80f,
            c.a);
    }
}
