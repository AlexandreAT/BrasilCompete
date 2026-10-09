import { SafeAreaView } from 'react-native-safe-area-context';
import { styled } from 'styled-components/native';

import { Theme } from '@/shared/themes/Theme';

import { SAFE_AREA_EDGES } from './types';

export const Content = styled.ScrollView.attrs({
  contentContainerStyle: {
    paddingBottom: 24,
    paddingHorizontal: 20,
  },
  showsVerticalScrollIndicator: false,
})``;

export const SafeArea = styled(SafeAreaView).attrs({
  edges: SAFE_AREA_EDGES,
})`
  background-color: ${Theme.colors.background};
  flex: 1;
`;

export const ScreenTitle = styled.Text`
  color: ${Theme.colors.primaryText};
  flex-shrink: 1;
  font-family: ${Theme.fonts.bold};
  font-size: 24px;
  letter-spacing: -0.6px;
`;

export const SeeAllButton = styled.TouchableOpacity.attrs({
  activeOpacity: 0.65,
})`
  align-items: center;
  flex-direction: row;
  gap: 3.5px;
  padding: 7px 0 7px 10.5px;
`;

export const SeeAllText = styled.Text`
  color: ${Theme.colors.primaryGreen};
  font-family: ${Theme.fonts.semiBold};
  font-size: 14px;
`;

export const TitleRow = styled.View`
  align-items: center;
  flex-direction: row;
  justify-content: space-between;
  margin-top: 21px;
`;

export const seeAllIcon = {
  color: Theme.colors.primaryNavy,
  size: 21,
  strokeWidth: 2.4,
};
