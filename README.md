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