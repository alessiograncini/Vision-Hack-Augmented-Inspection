
# Augmented Inspection and Teleoperation Project

![AR Cover Image](Cover/cover_image.png)

## Overview

This project is developed using **Unity 2022.3.38f1** and **Apple Vision Pro OS Template 1.3.1**. It explores the potential of **augmented reality (AR)** for **inspection and teleoperation** tasks, enabling users to interact with a physical environment through immersive augmented interfaces.

In this last 2 years we assisted to an exponential growth in the robotic ecosystem. We believe XR can play an instrumental role in improving how to remote control robot and capture meaningful data that can improve robotics in the long term. 

The project leverages Unity's AR capabilities in combination with the Apple Vision Pro, focusing on enhanced workflows for **remote monitoring**, **robotic control**, and **visual feedback**.

See our full google slide presentation and video
https://docs.google.com/presentation/d/19J8hxo8IiWyNzzQIFGkX9_B_qeUiCcJHosvsiL6Bcvs/edit?usp=sharing

## Features

- **Augmented Inspection**: Allows users to inspect environments, machinery, or objects with overlaid data in real time using the Apple Vision Pro.
- **Teleoperation Interface**: Remote control and monitoring of a connected robotic system via the AR interface.
- **Head and Hand Tracking**: Full support for Vision Pro’s head and hand gestures to interact with the augmented environment.
- **Synthetic Data Collection**: Captures user interactions and environmental data for further training and optimization.
- **Visual Feedback**: Displays real-time data such as sensor information or point clouds overlaid on the physical environment.
- **Generative AI Multimodal**: Includes integration of Open AI Voice and Vision model for basic communication with Robot. In this way, tele operating the robot becomes more of a copiloting experience. 

## Requirements

- **Unity 2022.3.38f1** (or newer)
- **Apple Vision Pro OS Template 1.3.1**
- **Mac with Apple Silicon** (for testing and deployment)
- **Apple Vision Pro Headset** (for AR experience)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/augmented-inspection-teleoperation.git
   ```
2. Open the project in **Unity 2022.3.38f1**.
3. Ensure the **Apple Vision Pro OS Template 1.3.1** package is installed via the Unity Package Manager.
4. Build and deploy the project to your **Apple Vision Pro** device.

## Usage

This app is quite hard to distribute at this time as it relies on local connection between headset and robot. 

1. Wear the **Apple Vision Pro** headset.
2. Open the application via the Vision Pro launcher. 
3. Select the **Augmented Inspection** app to begin inspecting environments.
4. Use hand gestures and to interact with the input pad and pilot the robot. 
5. Use voice to pilot the Boston Dynamics Spot robot. 
6. Make sure the Luxonis OAK-D Camera is mounted on the robot for real time world reconstruction.

Luxonis OAK-D
https://shop.luxonis.com/collections/oak-cameras-1?srsltid=AfmBOood1hAUhpTlT6GobIRkmylOXXePJiz_ToExvFjSrsC5q7USR57n

## Credits 

This project was realized for the Vision Hack Hackathon 09/13-15/2024 

Connect with us 

Alessio Grancini, AR/VR Engineer
X.com/alessiograncini

Arthur Baney, AR/VR Engineer
X.com/arthurzbaney

Vincent Trastour, AR/VR Engineer
X.com/vincent_trasto

A special thanks to 

Circuit Launch, an amazing facility we are members of, based in Oakland. 
https://www.circuitlaunch.com/

Robonomics, the most supportive team providing the robot to explore teleoperation
https://robonomics.network/


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
