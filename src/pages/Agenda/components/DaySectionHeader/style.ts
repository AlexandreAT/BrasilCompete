import { styled } from 'styled-components/native';

import { shadow } from '@/shared/styles/shadow';
import { Theme } from '@/shared/themes/Theme';

export const Container = styled.View`
  align-items: center;
  flex-direction: row;
  gap: 10.5px;
  margin-bottom: 10.5px;
  margin-top: 14px;
`;

export const IconCircle = styled.View`
  align-items: center;
  background-color: ${Theme.colors.white};
  border-radius: 9999px;
  height: 38px;
  justify-content: center;
  width: 38px;
  ${shadow({ elevation: 3, offsetY: 2, opacity: 0.08, radius: 5 })}
`;

export const Title = styled.Text`
  color: ${Theme.colors.primaryText};
  font-family: ${Theme.fonts.bold};
  font-size: 21px;
`;

export const sectionIcon = {
  color: Theme.colors.primaryGreen,
  size: 21,
  strokeWidth: 2,
};
