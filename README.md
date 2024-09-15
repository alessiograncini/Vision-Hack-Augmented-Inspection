
# Augmented Inspection and Teleoperation Project

![AR Cover Image](Cover/cover_image.png)

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Setup](#setup)
- [Usage](#usage)
- [Commands](#commands)
- [Voice Commands](#voice-commands)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Credits](#credits)
- [Changelog](#changelog)
- [License](#license)

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
- **OpenAI API Key** (for AI integration)
- **A Robot** (e.g., Boston Dynamics Spot)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/augmented-inspection-teleoperation.git
   ```
2. Open the project in **Unity 2022.3.38f1**.
3. Ensure the **Apple Vision Pro OS Template 1.3.1** package is installed via the Unity Package Manager.
4. Build and deploy the project to your **Apple Vision Pro** device.

## Setup

This project works by being linked with a robot API. The `ManagerController` and `ManagerVideo` components take a URL of a server running on the robot. Here is the list of endpoints that are needed:

### Endpoints

- **ImageReceiver**:
  - `GET /get_frame`: Fetches the current frame from the robot's camera.

- **RobotController**:
  - `POST /command`: Sends a command to the robot. The payload should be in JSON format and can include movement commands.

There is an example of a Python web server in the folder at the root called `SpotCoreServer` that you can use to set up the necessary API endpoints.

### OpenAI Integration

You will also need an OpenAI account and an API key to access OpenAI's services. This key will be set in the `OpenAIService` component from the `ManagerAudio-http` GameObject.

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

## Commands

### Available Commands

- **start**: Starts the robot.
- **stop**: Stops the robot.
- **velocity**: Moves the robot with specified velocities and duration.

### Building the Body of the POST Request

The body of the POST request should be in JSON format. Here are examples for each command:

#### Start Command

```json
{
   "command": "start",
   "args": null
}
```

#### Stop Command

```json
{
   "command": "stop",
   "args": null
}
```

#### Velocity Command

For the `velocity` command, you need to provide the velocity arguments:

- `v_x`: Velocity in the x-direction (forward/backward).
- `v_y`: Velocity in the y-direction (left/right).
- `v_rot`: Rotational velocity.
- `cmd_duration`: Duration of the command.

Example:

```json
{
"command": "velocity",
   "args": {
      "v_x": 1.0,
      "v_y": 0.0,
      "v_rot": 0.5,
      "cmd_duration": 2.0
   }
}
```


## Voice Commands

### Using Voice Commands

The voice command functionality uses [Whisper.unity](https://github.com/Macoron/whisper.unity) and the model tiny from Whisper by OpenAI, which has been optimized as `ggml-tiny.bin`. The voice commands are parsed by OpenAI to detect specific commands.

### Available Voice Commands

- **"move forward"**: Moves the robot forward.
- **"move backward"**: Moves the robot backward.
- **"turn left"**: Turns the robot left.
- **"turn right"**: Turns the robot right.
- **"stop"**: Stops the robot.
- **"tell me what you see"**: Describes the current view from the robot's camera.
- **"start"**: Starts the robot.
- **"stop"**: Stops the robot.

### How It Works

1. **Voice Input**: The user speaks a command.
2. **Transcription**: The spoken command is transcribed using Whisper.unity.
3. **Command Parsing**: The transcribed text is sent to OpenAI for parsing to detect the command.
4. **Command Execution**: The detected command is executed by the `CommandExecutor` component.

## Troubleshooting

### Common Issues

1. **Unity Package Installation Issues**
   - Ensure you are using the correct version of Unity.
   - Check the Unity Package Manager for any missing dependencies.

2. **Connection Issues with the Robot**
   - Verify the robot's IP address and ensure it is reachable.
   - Check the network settings and firewall configurations.

3. **Voice Command Issues**
   - Ensure the microphone is properly configured and working.
   - Verify that the OpenAI API key is correctly set in the `OpenAIService` component.

For more help, please refer to the [issues section](https://github.com/yourusername/augmented-inspection-teleoperation/issues) or contact us directly.

## Contributing

We welcome contributions to this project! Please follow these steps to contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes and commit them (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Open a pull request.

Please ensure your code follows our coding guidelines and includes appropriate tests.

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

## Changelog

### [1.0.0] - 2024-09-15
- Initial release for the Vision Hack Hackathon.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
