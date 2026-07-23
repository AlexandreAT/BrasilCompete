# Brasil Compete

Aplicativo mobile para centralizar eventos esportivos internacionais com
representação brasileira.

O projeto usa React Native, TypeScript, Expo SDK 54, Expo Router, TanStack Query
e NativeWind. As decisões arquiteturais e a visão do produto estão detalhadas
em [PROJECT_GUIDE.md](./PROJECT_GUIDE.md).

## Requisitos

- Node.js 20.19 ou superior;
- npm;
- Expo Go instalado no celular.

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

Outros comandos disponíveis:

```bash
npm run android
npm run ios
npm run web
npm run lint
npm run typecheck
```

## Organização

- `src/index.ts`: inicia o Expo Router;
- `src/App.tsx`: componente principal do aplicativo;
- `src/AppRoutes.tsx`: navegação principal;
- `src/pages/`: páginas do aplicativo;
- `app/`: arquivos de ligação exigidos pelo Expo Router;
- `assets/`: imagens e demais arquivos estáticos.

Os arquivos `babel.config.js`, `metro.config.js`, `tailwind.config.js`,
`global.css` e `nativewind-env.d.ts` são a configuração mínima necessária para
usar NativeWind, conforme definido no guia do projeto.
