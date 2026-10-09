import type { BottomTabNavigationOptions } from '@react-navigation/bottom-tabs';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { getTabBarStyle, tabBarColors, tabItem } from './style';
import { MIN_TAB_BAR_BOTTOM_INSET, TAB_SCREENS } from './types';

export function useTabRoutes() {
  const { bottom } = useSafeAreaInsets();
  const bottomInset = Math.max(bottom, MIN_TAB_BAR_BOTTOM_INSET);

  const screenOptions: BottomTabNavigationOptions = {
    ...tabBarColors,
    headerShown: false,
    tabBarHideOnKeyboard: true,
    tabBarItemStyle: tabItem,
    tabBarStyle: getTabBarStyle(bottomInset),
  };

  return { screenOptions, screens: TAB_SCREENS };
}
