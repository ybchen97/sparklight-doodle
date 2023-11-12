# SparkLight Doodle - Immerse The Bay Hackathon 2023

SparkLight Doodle is a simple hand-tracking based VR game where you kill enemies
by drawning the symbol that appears over their heads.

## Contributors
Yukun Song, Kathy Zhuang, Aria Kang, Yuanbo Chen

## Gameplay design

- The core logic of the gameplay is an infinite enemy spawner where you try your
  best to kill as many monsters as possible before dying.
- The `MonsterSpawner` class tracks and manages the number of monsters current
  on the field.

## Documentation for Hand Tracking

- The schemes of hand models are created and synced by the Ultraleap API
  providers.
- The functionality we implemented allow to track and record the drawing of one
  finger on another hand's palm.
- We also implemented a server on the branch of "ultraleap-processing", which
  utilizes neural network to detect the shape of drawing.
- By recognizing the drawing, we send out corresponding attacks to knockback the
  little monsters. The logic of sending out attacks is done by detecting
  multiple touches of one hand on another (multiple finger tips touching the
  palm of the other hand).
- When placed properly, the Ultraleap camera creates high fidelity in the
  tracking process and gives accurate (85%) predicting results of the three
  types of attack - `horizontal_line`, `vertical_line`, and `hat_caret`.
