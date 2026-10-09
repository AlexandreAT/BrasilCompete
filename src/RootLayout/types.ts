import { Inter_400Regular } from '@expo-google-fonts/inter/400Regular';
import { Inter_500Medium } from '@expo-google-fonts/inter/500Medium';
import { Inter_600SemiBold } from '@expo-google-fonts/inter/600SemiBold';
import { Inter_700Bold } from '@expo-google-fonts/inter/700Bold';

import { Theme } from '@/shared/themes/Theme';

export const APP_FONTS = {
  [Theme.fonts.regular]: Inter_400Regular,
  [Theme.fonts.medium]: Inter_500Medium,
  [Theme.fonts.semiBold]: Inter_600SemiBold,
  [Theme.fonts.bold]: Inter_700Bold,
};
