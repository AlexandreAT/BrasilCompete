import { StatusBar } from 'expo-status-bar';

import { AppRoutes } from './AppRoutes';
import '../global.css';

export function App() {
  return (
    <>
      <AppRoutes />
      <StatusBar style="dark" />
    </>
  );
}
