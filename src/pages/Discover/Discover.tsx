import { Text, View } from 'react-native';

export function Discover() {
  return (
    <View className="flex-1 items-center justify-center bg-white px-6">
      <Text className="text-3xl font-bold">Descobrir</Text>
      <Text className="mt-2 text-center">
        A busca por esportes, competições e participantes aparecerá aqui.
      </Text>
    </View>
  );
}
