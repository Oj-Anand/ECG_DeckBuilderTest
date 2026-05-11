# Card Saving System

A deckbuilding metagame loop for ECG's Deckbuilding and Save system test.

## Tech

- Unity 6 (600.3.6f1), URP
- DoTween for animation sequencing
-JSONBIN.io as remote persistence backend

## Project Structure 
Assets/
|-- Art/Cards/ -> Frame, backing, and 15 character illustrations
|-- Prefabs/Cards -> Card prefab 
|-- ScriptableObjects/Cards/ -> The 15 card data strctures
|-- Scripts/ 
|   |-- Data/ -> CardData (SO definition), CardType enum
|   |-- Views/ CardView (prefab monobehaviour)
|   |-- Services/ -> (Part 3 Api client servies and deck repo)
|   |-- UI -> (Part 2 Scene controllers)
|-- Scenes/ -> MainMenu, DeckBuilder, DeckViewer

## Part 1: Card Architecture 

A card consists of: 

-- **`CardData`** (ScriptableOject) defines what a card is - id, name, type, cost, attack, health, illustration and description. 
-- **`CardView`** (MonoBehaviour on the card prefab root) defines how a card looks - it binds a `CardData` to UI elements via a `Bind(CardData)` method. 
-- **The Card Prefab** is a single reusable visual that any `CardData` can populate 

The goal here was to decouple persisence from the visual representation of the card, so the saved deck format only needs to store card IDs. Also makes adding more cards a no code task. 

### Card Types 

Cards are typed as either a `Character` or a `Spell` via a `CardType` enum. The view uses this to conditionally render the stat (attack and health) badges since spells dont have any stats. 

### Visual Layout 

The example card in the brief showed only a cost number, no Attack/Health values. I chose to display stats on the card face (bottom corners, styled like an off brand HearthStone card) because gameplay infomration should be readily availble to the player. This matches conventions that players should be familiar from other card games. the badges can be given a more obvious red for attack, green for health if the project is aimed at new/beginer players. Spell cards use the same prefab but they exchange the stat badges for a spell badge, so the player knows this is a distinct card they would use in a different way than character cards. 

### Prefab Construction

Cards use a ***World Space Canvas* parented to a 3D root, which gives us good visuals using UI images and TMP elements and a 3D object that can be rotated and animated spatially in the deck builder. The prefab has seperate Front and Back faces so the card can be flipped during the draw animation ill implement in part 2. The card root's pivot is at the bottom center so cards can fan naturally around the player hand's pivot point. 


## Part 2a: Main Menu

THe Main menu handles UUID generation/lookup and routes the player to either the Deck Builder or Deck Viewer scenes. 

### User Identity flow 

UUIDs are genrated via `System.Guid.NewGuid()` and stored in `PlayerPrefs` under a single key (`user_id`) to follow the brief's constraint of using PlayerPrefs solely for UUID
- **New User** generates a fresh UUID, persists it, and loads the Deck Builder scene. 
-**Continue** loads the Deck Viewer scene using the existing UUID. 

The Continue button is disabled rather than hidden when no UUID exists so the user is aware that this is an option that exists once they have a session to return to. 

### Architecture 

`MainMenuController` is a single monobehaviour that : 

1. Reads/Writes the UUID via PlayerPrefs
2. Drives entry/exit animation via DoTween 
3. Triggers scene transitions 

Constants (`UserIdKey`, scene names) are declared at the top of the controller

### UI Layout 

The menu uses a screen space - camera canvas with a canvas scaler set to 1920x1080 reference resolution to ensure the layot adapts cleanly across displays. Buttons are arranged via a `VerticalLayoutGroup` + `ContentSizeFilter` combo which auto sizes them and centers them with consistent spacing. This also makes adding more menu options later convenient. 

Title and button groups are wrapped in `CanvasGroup` components so each sections al

### Animation 

All menu animations use DoTween: 

- **Entry Sequence:** On scene load the title fades in and slides down from above; once it lands, the button group slides up from the bottom of the screen. The stagger adds a bit of a flourish to the menu reveal which adds to the general excitement the player would feel at the prospect of booting up a new game. 
- **Hover Effect:** Buttons scale up 1.05x on `IPointerEnterHandler` and ease back on exit. The script isnt exclusive to the menu and can be used on any UI element. 
- **Exit sequence:** When a button is clicked, the menu fades out and slides offscreen before the next scene loads. Buttons are disabled at the start of the exit to prevent double-clicks during the fade. 

I sequenced the animations using `DoTween.Sequence()` with `Append/Join`. 

### Known Considerations 

- Hover tween components dont explicitly `DoKill()` on destroy. This is safe within the menu's lifecycle for this demo, as the buttons exist for the entire scene. For a more complex UI where elements are dynamic created/destroyed an `OnDestroy` cleanup hook would be appropriate. 

## Part 2b: Deck Builder

The deck builder is the heart of the metagame loop: spawn a shuffled deck, draw cards one at a time with a multi-stage animation, fan them into a hand, and once the hand limit is reached, save to the remote backend.


### Architecture

The scene is split into focused components each of which has a single responsibility; the controller orchestrates the flow between them:

- **`DeckStack`** spawns and manages the visual deck of face-down cards. Exposes `BuildDeck()`, `PopTopCard()`, and a `Count` property.
- **`HandLayout`** arranges drawn cards in a fan around a hand pivot. Exposes `GetSlotForNextCard()` and `AddCard()`.
- **`CardAnimator`** performs the draw -> flip -> focus -> settle sequence on a single card.
- **`DeckBuilderUI`** owns the screen-space UI (hand counter, save/discard buttons, loading overlay).
- **`DeckBuilderController`** is the orchestrator. It owns the high-level flow (draw -> animate -> add to hand -> check completion -> save) and delegates each step to one of the above.

### Deck Spawning

`DeckStack.BuildDeck()` instantiates one card per `CardData` in the registry, shuffles them with a Fisher-Yates shuffle, and stacks them at the deck anchor with a small Y offset between cards to prevent z-fighting. The deck is built from a `CardRegistry` ScriptableObject which holds references to all 15 cards. This is a single point of access for any system that needs the card pool.

Fisher-Yates was chosen over the common `OrderBy(x => Random.value)` shortcut because the latter is statistically biased and produces noticeably non-uniform shuffles on small lists.

### Draw Animation

The draw animation is a single DOTween `Sequence` with multiple stages:

1. **Lift** -> the card rises off the deck (~0.2s). Anticipation frame.
2. **Travel + Flip** -> the card moves to the focus anchor while rotating face-up and scaling up (~0.5s). Multiple tweens running in parallel via `Sequence.Join()`.
3. **Hold** -> the sequence pauses at the focus position. A coroutine watches a flag set mid-sequence, pauses the tween, waits for the player's click, and resumes.
4. **Settle** -> the card moves to its target slot in the hand fan, scaling down and rotating to match its position in the fan (~0.4s).

The coroutine pattern was utilized to implement the hold stage as DOTween itself doesn't have a "wait for input" primitive. I used a single declarative sequence that keeps the full animation visible in one place. 

### Hand Fanning

`HandLayout` computes each card's target position, rotation, and scale based on its index and the total hand size. Adding a card recomputes the layout for *every* card and tweens them all to their new targets which is a declarative pattern (compute target from state) rather than imperative (push/pop adjustments).

This means adding the 4th card causes cards 1, 2, and 3 to shift slightly to make room, all in parallel, with no special-case code becaus edge cases are the worst.

The fan math also compensates for the card prefab's center pivot: each computed position is offset upward along the card's local up axis, so the visual bottom of each card makes a nice fan shape.

### Hand Limit and Completion

The brief specifies an 8-card hand limit. Once `handLayout.Count >= HandLimit`, the controller sets `_isComplete` to true (which gates further input in `Update`) and shows the completion UI which includes the Save and Discard buttons fading in via DOTween.

- **Save** sends the deck to the remote backend via `IDeckRepository`, then returns to the main menu on success.
- **Discard** returns to the main menu without saving.

### Known Considerations

- The card prefab's pivot is at its center rather than its bottom. The fan math compensates for this with a runtime offset, but a future iteration would refactor the prefab to have a bottom-anchored pivot, which would simplify the layout math.
- The `IsClickingDeck()` check in the controller is a stub that returns true for any click. A production version would `Physics.Raycast` from the camera through the cursor and verify the hit collider belongs to the top deck card.

## Part 2c: Deck Viewer

The deck viewer fetches the user's saved decks from the remote backend on scene load and displays them in a scrollable list. Each deck shows its 8 cards as thumbnails, and clicking a thumbnail opens a focus overlay with a larger view.

### Architecture

- **`DeckViewerController`** drives the scene: triggers the API call, handles the response, populates the scroll list, and routes thumbnail clicks to the focus overlay.
- **`DeckEntryView`** is a prefab representing one row in the scroll list. Bound with a deck number and a list of `CardData`, it instantiates one `CardThumbnail` per card.
- **`CardThumbnail`** is a prefab representing one small card preview (illustration + name). Raises an event when clicked.
- **`CardFocusOverlay`** is a full-screen UI overlay that displays a single card at large size when a thumbnail is clicked. Click anywhere to dismiss.

Thumbnails were built as lightweight Image + TextMeshPro UI elements rather than reusing the world-space `CardView` prefab. This is cleaner because thumbnails are pure 2D screen-space UI; nesting a world-space canvas inside a screen-space scroll list would (and did) cause rendering quirks.

### API Loading Flow

On `Start`, the controller calls `IDeckRepository.LoadDecks(userId, ...)`. While the request is in flight, a loading overlay is shown. On success, the response is mapped from card IDs back to full `CardData` via the registry, and a `DeckEntryView` is spawned per deck. On error, an error message is displayed.

The empty state and error state share a single message label for simplicity. In a production app, transient errors would use a separate toast/banner pattern that auto-dismisses, while empty state remains persistent until the underlying state changes.

### Click-to-Focus

Each `CardThumbnail` raises an `OnClicked` event when clicked. The `DeckEntryView` forwards these events upward via its own `OnCardClicked` event. The controller subscribes to this and calls `CardFocusOverlay.Show(cardData)`.

The focus overlay uses a `CanvasGroup` to fade in the dim backdrop and the focused card together, with `interactable` and `blocksRaycasts` toggled on/off so the dismiss-on-click behavior only works when the overlay is visible.

### Known Considerations

- The focused card view rebuilds the card layout in screen-space UI rather than reusing the world-space `CardView` prefab, due to the rendering boundary between screen-space and world-space canvases. A future iteration could use a dedicated camera + RenderTexture to display the actual prefab inside the screen-space overlay.

## Part 3: Persistence and API

### Architecture

Persistence is split across three layers:

- **DTOs** (`UserCollection`, `UserRecord`, `DeckRecord` in `Data/`) are plain `[Serializable]` classes that match the wire format. They contain only primitive types and lists.
- **`IDeckRepository`** is the persistence contract: `SaveDeck()` and `LoadDecks()`, both async, both report results via callbacks.
- **`JsonBinDeckRepository`** is the concrete implementation that talks to JSONBin via `UnityWebRequest`.
- **`UserSession`** is a thin static wrapper around the PlayerPrefs UUID lookup.

Controllers depend on the interface, not the implementation. Swapping JSONBin for a different backend (PlayFab, Firebase, a custom REST API) would require changing only `JsonBinDeckRepository`. None of the controllers know which backend they're talking to.

### Why Callbacks Over Async/Await

The repository methods take `Action<T>` callbacks rather than returning `Task<T>`. This was a deliberate choice: coroutines and `UnityWebRequest` naturally complement callback-based APIs, and async/await in Unity comes with caveats (deadlocks, exception swallowing) that were not worth the risk in a short build window. 
### Data Format

The brief specifies a wire format like:

```json
{
  "user_id": "550e8400-e29b-41d4-a716-446655440000",
  "decks": [["card_id_1", "card_id_4", ...]]
}
```

The implementation wraps each deck in a `DeckRecord` object rather than using bare arrays:

```json
{
  "user_id": "...",
  "decks": [
    {"card_ids": ["card_id_1", "card_id_4", ...]}
  ]
}
```

This is because Unity's `JsonUtility` does not support serializing `List<List<string>>` directly. The structure preserves the same information and could be migrated to the bare-array format with Newtonsoft.Json if required. JSON-serializable DTOs use `snake_case` field names to match the wire format, while this deviates from C# conventions it is necessary for `JsonUtility` to round-trip correctly.

### Multi-User Bin Strategy

Since the brief restricts PlayerPrefs to UUID storage only, the implementation uses a single shared JSONBin bin to hold all users. The bin contains a `UserCollection` with one `UserRecord` per user, keyed by UUID. On save, the repository fetches the current bin contents, locates or creates the user's record, appends the new deck, and writes the entire bin back.

This has a known concurrency limitation (last-write-wins if two users save simultaneously), which is acceptable for a test but would need to be addressed in production with optimistic concurrency control or per-user bins.

### Loading and Error States

Both the deck builder and deck viewer show a loading overlay during API calls, implemented as a semi-transparent panel with a manually-rotated spinner Image. This was chosen over a built-in animation system because it's three lines of code (`Rotate(0, 0, -180f * Time.deltaTime)` in `Update`) and avoids the overhead of an Animator component for a single rotation.

Errors are caught at the repository layer (HTTP failures, request errors) and surfaced to the controllers via the `onError` callback. The controllers display a human-readable message via the UI's `ShowError` method.

### Known Considerations

- The JSONBin master key is stored in the Inspector field of `JsonBinDeckRepository`. In production, this should be loaded from a gitignored config asset or environment variable to prevent leaking in source control.
- The save flow does a full bin fetch + write per save which wouldnt scale for many users, but it is sufficient given the time constraints for the test.

## Running the Project

1. Clone the repo and open in Unity 6 (6000.3.6f1) with URP.
2. Open `Assets/Scenes/MainMenu.unity` as the entry scene.
3. To test the API integration end-to-end, fill in your own JSONBin bin ID and master key on the `_Services` GameObject in both `DeckBuilder.unity` and `DeckViewer.unity` scenes. Initialize the bin contents to `{"users": []}`.
4. Press Play. The full flow is: New User -> draw 8 cards (click deck, click again to dismiss focus) -> Save -> Continue from menu -> view saved decks.

## Final Notes

This was a 4-6 hour test executed in approximately 8 hours of focused work. Some polish items (a more elaborate focused-card UI, deck card hover effects) were deprioritized in favor of completing every functional requirement and demonstrating the architectural patterns the brief explicitly evaluates: separation of concerns, persistence layer abstraction, and animation sequencing.

## Post - Submission Refinements

After the initial submission I returned to address 4 issues spotted during a quality pass. 

- **Multi-deck support** The viewer oriignally had no entry point back into the deck builder scene for returing users, allowing users to accumulate multiple decks under the same UUID.
- **Focus mode event subscription** `DeckViewerController.DisplayDecks` was missing the `entry.OnCardClicked += HandleCardClicked` subscription, leaving the event chain incomplete. Adding the subscription and the handler method restored the click-to-focus behavior.
- **Deck orientation.** Deck cards now lie flat on the table with backs facing up, via an X-axis rotation in `DeckStack`.
- **Hand fan readability.** The fan radius was increased so all 8 cards are clearly visible and not stacked on top of one another.

An `OnDrawGizmosSelected` visualization was also added to `HandLayout` to support live in-editor tuning of the fan parameters.

The original submission timestamp is preserved in the git log; these refinements are available for review but should be considered optional context rather than part of the timed submission.