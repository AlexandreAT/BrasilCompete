import { styled } from 'styled-components/native';

import { Theme } from '@/shared/themes/Theme';

export const Banner = styled.ImageBackground.attrs({
  imageStyle: {
    borderRadius: 24,
  },
  resizeMode: 'cover',
})`
  background-color: ${Theme.colors.darkNavy};
  border-radius: 21px;
  height: 198px;
  margin-top: 10.5px;
  overflow: hidden;
`;

export const Content = styled.View`
  height: 100%;
  justify-content: center;
  padding-left: 14px;
  width: 68%;
`;

export const CountryHighlight = styled.Text`
  color: ${Theme.colors.primaryGreen};
`;

export const Description = styled.Text`
  color: ${Theme.colors.white};
  font-family: ${Theme.fonts.regular};
  font-size: 13px;
  line-height: 18px;
`;

export const HighlightLine = styled.View`
  background-color: ${Theme.colors.highlightYellow};
  border-radius: 3px;
  height: 4px;
  margin-bottom: 12px;
  margin-top: 11px;
  width: 48px;
`;

export const Overlay = styled.View`
  background-color: ${Theme.colors.heroOverlay};
  bottom: 0;
  left: 0;
  position: absolute;
  right: 0;
  top: 0;
`;

export const Title = styled.Text.attrs({
  adjustsFontSizeToFit: true,
  minimumFontScale: 0.86,
})`
  color: ${Theme.colors.white};
  font-family: ${Theme.fonts.bold};
  font-size: 22px;
  letter-spacing: -0.35px;
  line-height: 27px;
`;
