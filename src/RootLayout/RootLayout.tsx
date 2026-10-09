import { StatusBar } from 'expo-status-bar';

import { AppRoutes } from '@/AppRoutes/AppRoutes';

import { statusBar } from './style';
import { useRootLayout } from './useRootLayout';

export function RootLayout() {
  const { isReady } = useRootLayout();

  if (!isReady) {
    return null;
  }

  return (
    <>
      <AppRoutes />
      <StatusBar {...statusBar} />
    </>
  );
}
