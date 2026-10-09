import type { StatusBarProps } from 'expo-status-bar';

import { Theme } from '@/shared/themes/Theme';

export const statusBar: StatusBarProps = {
  backgroundColor: Theme.colors.background,
  style: 'dark',
};
