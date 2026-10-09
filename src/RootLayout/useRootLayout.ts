import { useFonts } from 'expo-font';
import * as SplashScreen from 'expo-splash-screen';
import { useEffect } from 'react';

import { APP_FONTS } from './types';

SplashScreen.preventAutoHideAsync();

export function useRootLayout() {
  const [fontsLoaded, fontError] = useFonts(APP_FONTS);
  const isReady = fontsLoaded || fontError !== null;

  useEffect(() => {
    if (isReady) {
      SplashScreen.hideAsync();
    }
  }, [isReady]);

  return { isReady };
}
