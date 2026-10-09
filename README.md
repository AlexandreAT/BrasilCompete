# Brasil Compete

Aplicativo mobile para centralizar eventos esportivos internacionais com
representação brasileira.

O projeto usa React Native, TypeScript, Expo SDK 54, Expo Router, TanStack Query
e styled-components. As decisões arquiteturais e a visão do produto estão
detalhadas em [PROJECT_GUIDE.md](./PROJECT_GUIDE.md).

## Requisitos

- Node.js 20.19 ou superior;
- npm;
- Para rodar no celular: Expo Go compatível com o SDK 54 (veja
  [Versão do Expo Go](#versão-do-expo-go));
- Para rodar sem celular: Android Studio com um emulador (veja
  [Execução no emulador Android](#execução-no-emulador-android)).

## Instalação

```bash
npm install
```

## Execução com Expo Go

```bash
npm start
```

Com o computador e o celular na mesma rede, abra o Expo Go e leia o QR Code
mostrado no terminal. Se a rede local bloquear a conexão, tente:

```bash
npm start -- --tunnel
```

Na primeira vez, o Expo pede para instalar o pacote `@expo/ngrok`; aceite. Em
redes corporativas, o firewall costuma bloquear a conexão direta, e o túnel ou o
emulador são os caminhos mais confiáveis.

### Versão do Expo Go

Cada versão do Expo Go abre projetos de um único SDK, e a versão das lojas
acompanha o SDK mais recente. Se o Expo Go mostrar "Project is incompatible
with this version of Expo Go", instale a versão do SDK do projeto (hoje, 54).

No Android, desinstale o Expo Go atual e instale o APK oficial pelo navegador
do celular:
<https://github.com/expo/expo-go-releases/releases/download/Expo-Go-54.0.8/Expo-Go-54.0.8.apk>.
Depois disso, não aceite atualizações do Expo Go pela Play Store. Outras
versões ficam em <https://expo.dev/go>.

## Execução no emulador Android

Roda tudo no próprio computador, sem celular, Wi-Fi ou cabo. Os comandos
abaixo são para o PowerShell do Windows.

### Preparação (uma vez por computador)

1. Instale o [Android Studio](https://developer.android.com/studio) na opção
   Standard, com o item Android Virtual Device marcado.
2. Crie as variáveis de ambiente do usuário:
   - `ANDROID_HOME` com a pasta do SDK (padrão: `%LOCALAPPDATA%\Android\Sdk`);
   - `%ANDROID_HOME%\platform-tools` no `Path`.
3. Abra um novo PowerShell e confirme com `adb --version`.
4. No Android Studio, abra More Actions → Virtual Device Manager → Create
   virtual device, escolha um telefone e uma imagem x86_64 recente (por
   exemplo, API 35).
5. Confira o nome do dispositivo criado:

   ```powershell
   & "$env:ANDROID_HOME\emulator\emulator.exe" -list-avds
   ```

### Abrir o app

1. Inicie o emulador (troque `NOME_DO_DISPOSITIVO`) e espere a tela inicial do
   Android aparecer:

   ```powershell
   Start-Process "$env:ANDROID_HOME\emulator\emulator.exe" -ArgumentList '-avd','NOME_DO_DISPOSITIVO'
   ```

2. Na pasta do projeto, rode:

   ```bash
   npm start -- --localhost
   ```

3. No terminal do Expo, aperte `Shift+A` e escolha o emulador. Na primeira vez,
   o Expo oferece instalar o Expo Go compatível com o SDK do projeto; aceite.

Com o `--localhost`, o Expo redireciona a porta do servidor para dentro do
emulador pelo `adb` (`adb reverse`), sem depender da rede, do Wi-Fi ou do
firewall.

Para uma conferência rápida sem Android, aperte `w` no terminal do Expo para
abrir a versão web. Ela não é idêntica à do Android.

### Problemas comuns no emulador

- **O emulador fecha sozinho logo depois de abrir.** Costuma ser
  incompatibilidade com o driver de vídeo (no Visualizador de Eventos do
  Windows aparece uma falha do `qemu-system-x86_64.exe`). Inicie com
  renderização por software:

  ```powershell
  Start-Process "$env:ANDROID_HOME\emulator\emulator.exe" -ArgumentList '-avd','NOME_DO_DISPOSITIVO','-gpu','swiftshader_indirect'
  ```

  O emulador fica mais lento. Se aparecer "isn't responding", toque em Wait.
- **O emulador não inicia por falta de aceleração.** Rode
  `& "$env:ANDROID_HOME\emulator\emulator.exe" -accel-check`. No Windows, é
  preciso ativar o recurso "Plataforma do Hipervisor do Windows" (exige
  administrador).
- **A porta 8081 já está em uso.** Acontece quando outro projeto React Native
  está com o servidor aberto. Aceite a porta sugerida pelo Expo ou escolha uma
  na hora de iniciar: `npm start -- --localhost --port 8082`.
- **O app abre no aparelho errado.** Com um celular no USB e o emulador ligados
  ao mesmo tempo, a tecla `a` usa o primeiro aparelho da lista. Use `Shift+A`
  e confira os aparelhos com `adb devices`.
- **O app não conecta sem `--localhost`.** A rede interna do emulador usa a
  faixa 10.0.2.x. Se o IP do computador estiver nessa mesma faixa, o endereço
  de rede não funciona dentro do emulador.
- **O Expo não consegue instalar ou abrir o app sozinho.** Faça manualmente,
  trocando `8081` pela porta mostrada pelo Expo e `emulator-5554` pelo nome
  listado em `adb devices`:

  ```powershell
  adb -s emulator-5554 install Expo-Go-54.0.8.apk
  adb -s emulator-5554 reverse tcp:8081 tcp:8081
  adb -s emulator-5554 shell am start -a android.intent.action.VIEW -d exp://127.0.0.1:8081 host.exp.exponent
  ```

  O APK é o mesmo indicado em [Versão do Expo Go](#versão-do-expo-go).

## Outros comandos

```bash
npm run android
npm run ios
npm run web
npm run lint
npm run typecheck
```

## Organização

- `src/index.ts`: inicia o Expo Router;
- `src/RootLayout/`: componente raiz (fontes, splash screen e barra de status);
- `src/AppRoutes/`: navegação principal;
- `src/TabRoutes/`: barra de abas;
- `src/pages/`: páginas do aplicativo;
- `src/shared/`: componentes, estilos e tema reutilizados;
- `src/utils/`: funções utilitárias;
- `app/`: arquivos de ligação exigidos pelo Expo Router, que só reexportam
  componentes de `src/`;
- `assets/`: imagens e demais arquivos estáticos.

Não crie uma pasta `src/app` (nem `src/App`): o Expo Router passaria a procurar
as rotas nela, e no Windows e no macOS maiúsculas e minúsculas são o mesmo nome.

### Estrutura de telas e componentes

Cada tela ou componente fica em uma pasta própria, com um arquivo por
responsabilidade:

```text
CountryFlag/
├── CountryFlag.tsx     só renderização
├── types.ts            tipos, props e constantes
├── style.ts            estilos com styled-components
└── useCountryFlag.ts   lógica da tela ou do componente
```

- Os estilos ficam sempre no `style.ts`, com `styled-components/native`: nada de
  estilo inline, `StyleSheet` ou componentes nativos (`View`, `Text`) estilizados
  direto no `.tsx`.
- Escolhas visuais que dependem de estado são decididas no hook e mapeadas no
  `style.ts` (por exemplo, `$state: 'active' | 'inactive'`), sem condicionais
  dentro dos estilos.
- Os arquivos que não teriam conteúdo não são criados (um componente sem lógica
  não tem hook).
- O que se repete fica centralizado em `src/shared/` (tema, sombras e
  componentes como `PlaceholderScreen`).
