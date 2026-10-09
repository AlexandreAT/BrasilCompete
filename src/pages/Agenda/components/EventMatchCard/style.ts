import { styled } from 'styled-components/native';

import { shadow } from '@/shared/styles/shadow';
import { Theme } from '@/shared/themes/Theme';

export const Card = styled.View`
  align-items: center;
  background-color: ${Theme.colors.white};
  border-radius: 21px;
  flex-direction: row;
  min-height: 152px;
  padding: 14px 10.5px;
  ${shadow({ elevation: 4, offsetY: 5, opacity: 0.08, radius: 10 })}
`;

export const Competition = styled.Text`
  color: ${Theme.colors.primaryText};
  font-family: ${Theme.fonts.medium};
  font-size: 10px;
  max-width: 100%;
  text-transform: uppercase;
`;

export const Details = styled.View`
  align-items: center;
  flex: 1.45;
  min-width: 0;
  padding: 0 7px;
`;

export const Divider = styled.View`
  background-color: ${Theme.colors.softBorder};
  height: 92px;
  width: 1px;
`;

export const Location = styled.Text`
  color: ${Theme.colors.secondaryText};
  font-family: ${Theme.fonts.regular};
  font-size: 11px;
  margin-top: 2px;
`;

export const Time = styled.Text`
  color: ${Theme.colors.darkNavy};
  font-family: ${Theme.fonts.bold};
  font-size: 27px;
  letter-spacing: -0.6px;
  line-height: 34px;
  margin: 3px 0;
`;

export const Venue = styled.Text`
  color: ${Theme.colors.primaryText};
  font-family: ${Theme.fonts.regular};
  font-size: 12px;
  max-width: 100%;
`;
