# gen-teste

Projeto Unity VR baseado no template de VR da Unity, configurado para **Meta Quest 3S** (OpenXR). A cena principal é um ambiente fechado com teto branco uniforme, porta funcional na parede sul e interação por controles XR.

Repositório: [github.com/andremarcelino-mp4/gen-teste](https://github.com/andremarcelino-mp4/gen-teste)

## Requisitos

| Item | Versão / nota |
|------|----------------|
| Unity | **6000.5.10f1** (Unity 6) |
| Módulos do Editor | Android Build Support, OpenJDK e SDK/NDK incluídos no Hub |
| Headset | Meta Quest 3S (também Quest 3 via o mesmo perfil OpenXR) |
| Conta / dispositivo | Modo desenvolvedor no headset e `adb` no PC para instalar o APK |

## O que o projeto faz

- Cena de build: `Assets/Scenes/SampleScene.unity`
- Rig XR com mãos e controles (XR Interaction Toolkit 3.5.1 + XR Hands)
- Locomotion: movimento suave, giro e teleport
- Sala do template fechada em quadrado (~10×10 m)
- Teto sólido branco (sem claraboia do mesh original)
- Porta de uma folha na **Wall_South**, com maçaneta de alavanca, `HingeJoint` e `XRGrabInteractable`
- Interruptor ao lado da porta (LED verde) para ligar/desligar a luz
- OpenXR + Meta Quest Support (incluindo `quest3s`) e perfil **Touch Plus**

## Estrutura

```text
Assets/
  Scenes/SampleScene.unity     # cena que entra no APK
  Scripts/WhiteRoom/           # porta (HingeGrabDoor) e interruptor (ToggleLightSwitch), se presentes
  VRTemplateAssets/            # template VR (rig, materiais, modelos)
  Samples/                     # XRI Starter Assets, Hands Interaction Demo, simulador
  XR/Settings/                 # OpenXR / Meta Quest
  XRI/Settings/                # XR Device / Interaction Simulator
Packages/manifest.json         # dependências UPM
Builds/Quest3S/                # APK gerado localmente (não versionado)
```

Pacotes XR principais (`Packages/manifest.json`):

- `com.unity.xr.interaction.toolkit` 3.5.1  
- `com.unity.xr.hands` 1.8.1  
- `com.unity.xr.openxr` 1.17.1  
- `com.unity.xr.meta-openxr` 2.5.1  
- `com.unity.render-pipelines.universal` 17.5.0  
- Input System 1.20.0  

## Abrir no Editor

1. Instale o Unity **6000.5.10f1** pelo Hub, com o módulo **Android**.
2. Clone o repositório e abra a pasta do projeto no Unity Hub.
3. Espere o import terminar e abra `Assets/Scenes/SampleScene.unity`.

## Testar no Editor (sem headset)

O sample **XR Interaction Simulator** (XRI 3.5.1) simula cabeça e dois controles no Play Mode.

1. Confirme em **Project Settings → XR Plug-in Management → XR Interaction Toolkit** (ou no asset `Assets/XRI/Settings/Resources/XRDeviceSimulatorSettings.asset`) que o prefab do simulador está atribuído e **Automatically Instantiate** ligado **só no Editor**.
2. Aperte **Play**.
3. Use a UI/teclado/mouse do simulador para olhar, mover os grips e pegar a porta.

No build Android o simulador **não** deve ser instanciado (`m_AutomaticallyInstantiateInEditorOnly`).

## Build para Quest 3S

Configuração já usada neste projeto:

- Plataforma: **Android**
- Scripting backend: **IL2CPP**
- Arquitetura: **ARM64**
- Min / Target SDK: **34**
- Package name: `com.DefaultCompany.VRTemplate`
- Cena: `SampleScene`

No Editor: **File → Build Settings → Android → Switch Platform → Build**, e grave um `.apk`.

APK gerado localmente (ignorado pelo Git):

```text
Builds/Quest3S/gen-teste.apk
```

## Instalar no headset

Com USB, ADB e modo desenvolvedor:

```bash
adb devices
adb install -r "Builds/Quest3S/gen-teste.apk"
```

No Quest, abra **Apps → Fontes desconhecidas** (ou equivalente) e inicie **gen teste**.

## Controles e interação

- **Grip / select** nos interactables (`XRGrabInteractable`): porta e objetos do template.
- **Teleport** nas áreas/âncoras da cena.
- **Interruptor**: select/poke na placa ao lado da porta.

## Licença e créditos

- Template e samples: Unity (VR Template, XR Interaction Toolkit, XR Hands).
- Código próprio da sala/porta: pasta `Assets/Scripts/WhiteRoom/` quando existir no working tree.

Não commite `Library/`, `Logs/`, `Builds/` nem `.apk` — já cobertos pelo `.gitignore`.
