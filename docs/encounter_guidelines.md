# Guidelines for Creating Khemri Encounter Files

This document outlines the structure and content requirements for individual encounter Markdown files (`.md`) within the "Echoes of Khemri" game concept. The goal is to create rich, interconnected encounters that build the world's lore and provide engaging scenarios using an Obsidian-native wiki structure.

## File Naming Convention

Use a clear, descriptive naming convention:
*   Encounters: Place encounter files in the `Encounters/` directory. Use names like `Encounters/discovery_encounter_1.md`, `Encounters/hazard_combat_[...]_1.md`, etc.
*   Entities (Locations, Factions, People, Items, Concepts): Place entity files in appropriate subdirectories under a main folder (e.g., `encounters-objects/`). The filename should be `Entity Name.md` (e.g., `encounters-objects/Locations/Khenet Oasis.md`, `encounters-objects/Factions/Scavengers of the Red Waste.md`, `encounters-objects/Items/Pouch with Shifting Seal.md`). Use underscores or spaces as preferred for filenames, but use the exact name and path in `[[Links]]`.

## Encounter File Structure

Each encounter file should follow this structure, using Markdown formatting:

```markdown
# [Encounter Type] Encounter [Number]: [Brief Descriptive Title]

*   **Encounter Name:** [A slightly more evocative name for display/reference]
*   **Background Story:** [Set the scene. Why is the NPC here? What led to this moment? Include specific place names [[Place Name]], NPC goals, recent events (weather, previous encounters), mentioning relevant factions [[Faction Name]] or people [[Person Name]]. Inject world flavour.]
*   **Setting Details:** [Describe the immediate environment using sensory details. Reference specific landmarks [[Landmark Name]]. Hint at the deeper history or precursor presence subtly.]
*   **Trigger:** [Specific action or condition that initiates the core of the encounter.]
*   **Effect on NPC:** [Immediate impact on the NPC(s). Reference specific items [[Item Name]] or concepts [[Concept Name]] if perceived.]
*   **NPC Action:** [What does the NPC do? Mention skills *[Skill Check: Skill Name (Difficulty)]* or player choices.]
*   **Outcome:** [Detail possible results.]
    *   *Success:* [...]
    *   *Partial Success:* [...]
    *   *Failure:* [...]
*   **Intrigue:** [Pose questions raised. Hint at mysteries, connections to [[lore.md]], specific precursors like [[Star-Weavers]], or other entities [[Entity Name]].]
*   **Combat:** [Yes/No.]
*   **Skill Learned:** [...]
*   **Item Received:** [List specific named items gained/lost. **Link unique/named items: `[[Item Name]]`.**]
*   **Player Interaction Point:** [Yes/No. Describe choice.]
*   **Lore Introduced:** [Bulleted list primarily composed of wikilinks to the specific entities introduced or relevant in this encounter: `[[folder/Entity Name]]`, `[[folder/Item Name]]`, etc. Can also include links to general sections in the main lore overview: `[[lore.md#Section Name]]`.]
```

## Content Guidelines

*   **Wiki Linking:** **Aggressively link all specific named entities** (places, factions, people, items, concepts) using `[[folder/Entity Name]]` notation upon first mention or where relevant. Ensure corresponding `.md` files are created for these entities in their appropriate folders.
*   **Specificity:** Use invented names, now linked (e.g., `[[Khenet Oasis]]`, `[[Scavengers of the Red Waste]]`, `[[encounters-objects/Items/Pouch with Shifting Seal]]`). Avoid generic terms.
*   **Entity Files:** Create dedicated `.md` files for each linked entity. **Organize these files into type-based subdirectories** (e.g., `Items/`, `Locations/`, `Factions/`, `Civilizations/`, `Figures/`) within a central entity folder (e.g., `encounters-objects/`). These files should contain relevant backstory, descriptions, known associations, and mysteries.
*   **`lore.md` Role:** This file serves as a **high-level overview and index**. Its sections (Locations, Factions, etc.) should primarily contain lists of links `[[folder/Entity Name]]` pointing to the dedicated entity files. It should retain introductory paragraphs and the detailed Precursor Epoch descriptions.
*   **Motivation:** Provide context within encounter descriptions.
*   **Precursor Integration:** Weave in hints and links related to [[Elder Ones]], [[Serpent Sovereigns]], [[Star-Weavers]], [[Chronomancers]] and their legacies.
*   **Sensory Details:** Use vivid descriptions.
*   **Intrigue:** Ensure encounters raise questions.
*   **Khemri Perspective:** Maintain the pre-industrial Khemri viewpoint when describing precursor phenomena.

By following these guidelines, we create a deeply interconnected and navigable knowledge base for the game world within Obsidian. 