# Quiz Editor Window for Unity

This Unity Editor extension provides a custom window for managing quiz levels. It allows you to create, modify, and delete quiz levels, assign sprites for questions and wrong answers, and define a list of words per level. The window uses a ScriptableObject to store and persist quiz data.

## Features

- **Dynamic Level Management:**  
  Create and remove quiz levels dynamically, with fields for configuring each level's properties.
- **Sprite Assignment:**  
  Easily assign sprites for questions and wrong answers.
- **Word List Editing:**  
  Set the number of words per level (up to 5) and manage the corresponding word list.
- **Automatic Asset Creation:**  
  The editor automatically creates the required ScriptableObject asset if it doesn't exist.
- **Delete Level Button:**  
  Each level entry includes a "Delete Level" button with confirmation, allowing you to remove levels as needed.

## Requirements

- **Unity Version:** Unity 6000.0.26f1 (with support for ScriptableObjects and custom Editor windows)
## Installation

1. **Download or Clone:**  
   Download or clone this repository to your local machine.
2. **Open the Editor Window:**  
   In Unity, go to **Window > Quiz Editor** to open the custom editor window.

## Usage

1. **Opening the Editor:**  
   Open the Quiz Editor window via the Unity menu: **Window > Quiz Editor**.
2. **Managing Levels:**  
   - Use the **Number of Levels** field to add or remove levels.
   - For each level, assign sprites for the question and wrong answer.
   - Set the number of words (limited to 5) and fill in the word fields.
   - Use the **Delete Level** button in a level's header to remove it (a confirmation dialog will appear).
3. **Saving Changes:**  
   The asset is automatically saved when changes occur, ensuring that modifications are saved.

## Demo Video

Below is the demo video for the Quiz Editor Window.

Watch the video on [YouTube](https://youtu.be/a3oLsd-d34U?si=JrW2WgoktC2IA_nJ).

## Android Build

[Android Build Link](https://github.com/09-hrick/Word-Game-Framework/releases/tag/Android).

## Live Demo

[Demo link](https://09hrick.itch.io/word-game-framework).
