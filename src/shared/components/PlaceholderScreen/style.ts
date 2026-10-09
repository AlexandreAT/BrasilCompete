import { styled } from 'styled-components/native';

import { Theme } from '@/shared/themes/Theme';

export const Container = styled.View`
  align-items: center;
  background-color: ${Theme.colors.white};
  flex: 1;
  justify-content: center;
  padding: 0 21px;
`;

export const Description = styled.Text`
  margin-top: 7px;
  text-align: center;
`;

export const Title = styled.Text`
  font-size: 26.25px;
  font-weight: bold;
  line-height: 31.5px;
`;
