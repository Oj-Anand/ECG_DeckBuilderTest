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
