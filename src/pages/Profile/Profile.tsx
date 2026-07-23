import { Text, View } from 'react-native';

export function Profile() {
  return (
    <View className="flex-1 items-center justify-center bg-white px-6">
      <Text className="text-3xl font-bold">Perfil</Text>
      <Text className="mt-2 text-center">
        Suas preferências e configurações aparecerão aqui.
      </Text>
    </View>
  );
}
