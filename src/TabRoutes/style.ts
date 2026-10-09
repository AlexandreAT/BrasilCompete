import type { ViewStyle } from 'react-native';

import { Theme } from '@/shared/themes/Theme';

const TAB_BAR_BASE_HEIGHT = 56;

const tabBar: ViewStyle = {
  backgroundColor: Theme.colors.white,
  borderTopColor: Theme.colors.softBorder,
  borderTopLeftRadius: 22,
  borderTopRightRadius: 22,
  borderTopWidth: 1,
  elevation: 12,
  paddingTop: 6,
  shadowColor: Theme.colors.darkNavy,
  shadowOffset: { width: 0, height: -4 },
  shadowOpacity: 0.08,
  shadowRadius: 10,
};

export const tabBarColors = {
  tabBarActiveTintColor: Theme.colors.primaryNavy,
  tabBarInactiveTintColor: Theme.colors.disabled,
};

export const tabItem: ViewStyle = {
  paddingVertical: 0,
};

export function getTabBarStyle(bottomInset: number): ViewStyle {
  return {
    ...tabBar,
    height: TAB_BAR_BASE_HEIGHT + bottomInset,
    paddingBottom: bottomInset,
  };
}
