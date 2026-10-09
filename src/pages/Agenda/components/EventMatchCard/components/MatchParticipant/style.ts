import { styled } from 'styled-components/native';

import { Theme } from '@/shared/themes/Theme';

export const PARTICIPANT_FLAG_SIZE = 56;

export const Container = styled.View`
  align-items: center;
  flex: 1;
  min-width: 0;
`;

export const CountryName = styled.Text`
  color: ${Theme.colors.primaryText};
  font-family: ${Theme.fonts.bold};
  font-size: 11px;
  margin-top: 8px;
  max-width: 100%;
  text-transform: uppercase;
`;
